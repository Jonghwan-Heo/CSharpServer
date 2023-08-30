using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;

namespace Server
{
    public class FlushConsolidationHandler : ChannelDuplexHandler
    {

        private readonly int explicitFlushAfterFlushes;
        private readonly bool consolidateWhenNoReadInProgress;
        private readonly Func<int> flushTask;
        private int flushPendingCount;
        private bool readInProgress;
        private IChannelHandlerContext ctx;
        private CancellationTokenSource nextScheduledFlush;

        /**
         * The default number of flushes after which a flush will be forwarded to downstream handlers (whether while in a
         * read loop, or while batching outside of a read loop).
         */
        public static readonly int DEFAULT_EXPLICIT_FLUSH_AFTER_FLUSHES = 256;

        /**
         * Create new instance which explicit flush after {@value DEFAULT_EXPLICIT_FLUSH_AFTER_FLUSHES} pending flush
         * operations at the latest.
         */
        public FlushConsolidationHandler()
            : this(DEFAULT_EXPLICIT_FLUSH_AFTER_FLUSHES, false)
        {

        }

        /**
         * Create new instance which doesn't consolidate flushes when no read is in progress.
         *
         * @param explicitFlushAfterFlushes the number of flushes after which an explicit flush will be done.
         */
        public FlushConsolidationHandler(int explicitFlushAfterFlushes)
            : this(explicitFlushAfterFlushes, false)
        {

        }

        /**
         * Create new instance.
         *
         * @param explicitFlushAfterFlushes the number of flushes after which an explicit flush will be done.
         * @param consolidateWhenNoReadInProgress whether to consolidate flushes even when no read loop is currently
         *                                        ongoing.
         */
        public FlushConsolidationHandler(int explicitFlushAfterFlushes, bool consolidateWhenNoReadInProgress)
        {
            if (explicitFlushAfterFlushes <= 0)
            {
                throw new Exception("explicitFlushAfterFlushes: "
                        + explicitFlushAfterFlushes + " (expected: > 0)");
            }

            this.explicitFlushAfterFlushes = explicitFlushAfterFlushes;
            this.consolidateWhenNoReadInProgress = consolidateWhenNoReadInProgress;
            flushTask = consolidateWhenNoReadInProgress ? () =>
            {
                if (flushPendingCount > 0 && !readInProgress)
                {
                    flushPendingCount = 0;
                    ctx.Flush();
                    nextScheduledFlush = null;
                } // else we'll flush when the read completes

                return 0;
            } : default(Func<int>);
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            this.ctx = context;
        }

        public override void Flush(IChannelHandlerContext context)
        {
            if (readInProgress)
            {
                // If there is still a read in progress we are sure we will see a channelReadComplete(...) call. Thus
                // we only need to flush if we reach the explicitFlushAfterFlushes limit.
                if (++flushPendingCount == explicitFlushAfterFlushes)
                {
                    FlushNow(ctx);
                }
            }
            else if (consolidateWhenNoReadInProgress)
            {
                // Flush immediately if we reach the threshold, otherwise schedule
                if (++flushPendingCount == explicitFlushAfterFlushes)
                {
                    FlushNow(ctx);
                }
                else
                {
                    ScheduleFlush(ctx);
                }
            }
            else
            {
                // Always flush directly
                FlushNow(ctx);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            // This may be the last event in the read loop, so flush now!
            ResetReadAndFlushIfNeeded(ctx);
            ctx.FireChannelReadComplete();
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            readInProgress = true;
            ctx.FireChannelRead(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception ex)
        {
            ResetReadAndFlushIfNeeded(ctx);
            ctx.FireExceptionCaught(ex);
        }

        public override Task DisconnectAsync(IChannelHandlerContext ctx)
        {
            // Try to flush one last time if flushes are pending before disconnect the channel.
            ResetReadAndFlushIfNeeded(ctx);
            return ctx.DisconnectAsync();
        }

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            ResetReadAndFlushIfNeeded(ctx);
            return ctx.CloseAsync();
        }

        public override void ChannelWritabilityChanged(IChannelHandlerContext ctx)
        {
            if (!ctx.Channel.IsWritable)
            {
                // The writability of the channel changed to false, so flush all consolidated flushes now to free up memory.
                FlushIfNeeded(ctx);
            }
            ctx.FireChannelWritabilityChanged();
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            FlushIfNeeded(ctx);
        }

        private void ResetReadAndFlushIfNeeded(IChannelHandlerContext ctx)
        {
            readInProgress = false;
            FlushIfNeeded(ctx);
        }

        private void FlushIfNeeded(IChannelHandlerContext ctx)
        {
            if (flushPendingCount > 0)
            {
                FlushNow(ctx);
            }
        }

        private void FlushNow(IChannelHandlerContext ctx)
        {
            CancelScheduledFlush();
            flushPendingCount = 0;
            ctx.Flush();
        }

        private void ScheduleFlush(IChannelHandlerContext ctx)
        {
            if (nextScheduledFlush == null)
            {
                // Run as soon as possible, but still yield to give a chance for additional writes to enqueue.
                nextScheduledFlush = new CancellationTokenSource();
                ctx.Channel.EventLoop.SubmitAsync(flushTask, nextScheduledFlush.Token);
            }
        }

        private void CancelScheduledFlush()
        {
            if (nextScheduledFlush != null)
            {
                nextScheduledFlush.Cancel(false);
                nextScheduledFlush = null;
            }
        }
    }
}

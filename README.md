# CSharpServer
DotNetty 기반 특정 port 를 사용한 TCP 소켓 연결

Protobuf 3 를 이용한 패킷 시스템 network.proto 를 통해 build_proto.bat 를 통한 패킷 코드화

WorldServer, Room, Field 형태의 채널, 필드 구조

Socket 연결을 통한 Session 연결 후 WorldPlayer Class 로 접속 유저 관리 유저는 Room 이라는 공간에 항상 종속 되며 Room 안에는 유저가 존재 가능한 위치가 특정되는 Field 가 존재하며 유저의 특정 Field 내 상태를 나타내는 Unit 이라는 객체가 이를 관리

Dapper 를 통한 MySql DB 관리 싱글톤 형식으로 접근

Log, Game, World, Global 등 여러 DB 를 사용 할 경우 접근 가능한 Context 단위로 분리 Log4net 기반 로그 시스템 어떤 Class 에서 어떠한 형식의 로그가 남는지 추적
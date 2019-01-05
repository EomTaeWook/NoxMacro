﻿
연결 프로세스의 화면을 분석해서 키보드나 마우스 매크로 설정하여 키보드나 마우스 이벤트 반복

<img src="https://github.com/EomTaeWook/EmulatorMacro/blob/master/Release/Resource/capture.png" width="100%"></img>

개발환경

    WPF, C#, .net 4.6.1
    
OS 버전

    Window 8.0 이상 Window 7에서는 작동안하는 기능이 있음.

사용 방법

    1.화면 캡쳐 버튼를 통하여 이미지 캡쳐
    
    2.마우스 키보드 이벤트 선택
    
        2.1마우스 이벤트인 경우 좌표 지정 해주시면 됩니다.
        
        2.2키보드 이벤트인 경우 Ctrl + c + v 이런식으로 조합키를 넣어주면 됩니다.
        
    3.주기적으로 이미지를 캡쳐할 프로세스를 선택하시면 됩니다.

설정(Config.json)

    1.Language 언어 : [Eng],[Kor]
    
    2.SavePath : 설정 리스트 save 경로
    
    3.Period : 연결된 프로세스의 이미지를 캡쳐하는 주기
    
    4.Similarity : 이미지 프로세싱 유사도

작업 목록

    1.UI 구현(구현완료)
    
    2.저장 구현(구현완료)
    
    3.불러오기 구현(구현완료)
    
    4.이벤트 트리거(키보드 완료, 구현완료) 테스트
    
    5.이미지 프로세싱(간단한건 구현완료, 정확도 향상 구현중)
    
    6.DPI 화면 확대 적용시 마우스 포인터 좌표계가 맞질 않음(해결 완료)
    
    7.DPI 확대시 이미지 프로세싱이 안되는 현상이 있음. 수정중

버그 레포팅

    enter0917@naver.com

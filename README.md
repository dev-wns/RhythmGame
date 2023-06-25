![VS](https://img.shields.io/badge/VS2022-v17.4.4-red?style=flat&logo=visualstudio)
![Unity](https://img.shields.io/badge/Unity-v2021.3.16f1-blue?style=flat&logo=unity)
![FMOD](https://img.shields.io/badge/FMOD-v2.01.11-brightgreen?style=flat&logo=FMOD)

# 1. 소개
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521254-e10cce6a-0735-48fe-94b8-ba8f311d1cd5.gif"> 

      유니티를 활용하여 제작한 건반형 리듬게임입니다.

# 2. 사용 방법
## 2.1. 채보 추가 방법

      게임 설치 경로에 있는 StreamingAssets\\Songs 폴더안의 데이터를 탐색하여 채보 리스트를 작성합니다.

      Sound, BGM, 게임 데이터 등이 포함되어있는 폴더를 해당 경로로 이동시켜 채보를 추가합니다.
      
## 2.2. 조작 방법

      # Arrow - 채보 및 옵션 선택
      # Enter - 게임 시작 및 옵션 설정 확인
      # Escape - 뒤로가기, 게임종료, 일시정지
      # Space - 게임 옵션
      # 1, 2 - 스크롤 속도 조절
      # F2 - 검색
      # F3 - 코멘트 작성
      # F5 - 채보 갱신
      # F10 - 시스템 설정
      # F11 - 키 설정

# 3. 기능
## 3.1. 변속
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521312-87d64431-838d-4022-8591-de8d8006d738.gif">    
            
      메인 BPM과 변경되는 BPM의 비율에 따라 노트의 속도가 달라집니다.
      
## 3.2. 속도 조절
* ### 스크롤 속도
<p align="center">
      
      노트는 선택한 채보의 BPM과 관계없이 스크롤 속도에 의존된 속도로 이동합니다.

      BPM이 서로다른 두 채보를 플레이할 때 스크롤 속도가 같으면 노트 또한 같은 속도로 이동합니다.
      
* ### 사운드 속도 ( 배속 )
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521330-a421424f-7475-4d59-884d-d0f7a7b272a4.gif">
<p align="center"> < 좌측( x0.7 ) 중앙( x1.0 ) 우측( x1.5 ) >
                
      변경 값에 따라 사운드 속도와 노트의 타격 타이밍이 함께 조정됩니다.
    
## 3.3. BGA
* ### Video
        
      BGA 중 가장 우선으로 채택되는 타입입니다.
      
      데이터의 VideoPath를 참조하여 해당 경로의 비디오 파일을 재생합니다.
      Unity VideoPlayer를 사용하여 .mkv, .mp4 등 여러 비디오 파일 형식을 지원합니다.
                     
* ### Sprite
      
      Video 타입보다 낮은 우선순위를 가지고 있습니다.
      
      데이터의 [Sprites] 하위 목록들을 참조하고 이미지를 한 장씩 교체해가며 배경을 구성합니다.
              
* ### Image
      
      영상 형식의 BGA 데이터가 없을 때 해당 채보의 타이틀 배경 이미지를 보여줍니다.
      
      모든 배경 데이터가 없을 시 미리 로딩된 기본 이미지를 보여줍니다.

외부의 모든 이미지 파일은 UnityWebRequest를 통해 로딩됩니다.

## 3.4. 키음

      # 배경음 영역 - 정의된 데이터에 의해 자동으로 재생되며 모드를 통해 제거된 키의 사운드는 배경음 영역에서 재생됩니다.
                   
      # 키음 영역 - 레인의 키를 입력하면 판정되어야 할 노트의 사운드를 재생합니다.

## 3.5. 게임 모드
* ### Random
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521388-11f838dd-b02c-4e13-8eb8-f15a3b2b49e4.gif">
<p align="center"> < 좌측( None ) 중앙( Basic ) 우측( Max ) >

      # Mirror - 레인이 좌우반전됩니다.

      # Basic Random - 모든 레인을 무작위로 섞습니다.
      
      # Half Random -중앙을 기준으로 절반씩 무작위로 섞습니다. 
      ( 홀수 키인 경우 중앙에 위치한 키는 제외하고 적용됩니다. )
      
      # Max Random - 노트마다 무작위 레인에 배치합니다. ( 계단 보정 )

* ### AutoPlay
      
      모든 판정을 자동으로 처리합니다.
      
* ### NoSlider
      
      롱노트를 일반노트로 변환하여 시작합니다.

* ### NoFail
      
      게임 오버가 되지 않습니다.

* ### FixedBPM
     
      메인 BPM만 사용하도록 고정하여 변속 기능을 제거합니다.

* ### 7K To 6K
     
      6키로 변환합니다.
  
# 4. 마치며
      
      이미지, 사운드 등 프로젝트에서 사용한 모든 리소스를 제외하여 포크 및 리포지토리 복제를 통해 실행하면 제대로 작동하지 않습니다.

![VS](https://img.shields.io/badge/VS2022-v17.8.1-red?style=flat&logo=visualstudio)
![Unity](https://img.shields.io/badge/Unity-v2022.3.5f1-blue?style=flat&logo=unity)
![FMOD](https://img.shields.io/badge/FMOD-v2.01.11-brightgreen?style=flat&logo=FMOD)

# 1. 소개
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521254-e10cce6a-0735-48fe-94b8-ba8f311d1cd5.gif"> 

      유니티를 활용하여 제작한 건반형 리듬게임입니다.

# 2. 사용 방법
## 2.1. 채보 추가 방법

      게임 설치 경로에 있는 StreamingAssets\\Songs 폴더안의 데이터를 탐색하여 채보 리스트를 작성합니다.

      Sound, BGA 등 게임 데이터가 포함되어있는 폴더를 해당 경로로 이동시켜 채보를 추가합니다.
      
## 2.2. 조작 방법
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/250523465-ce91bbec-10b0-4907-9c3c-16c4e749fc09.gif">
<p align="center"> < 로비 >

|   단축키   |     설명                 |   단축키   |         설명          |   단축키   |         설명         |
|:----------:|:-----------------------:|:----------:|:--------------------:|:----------:|:-------------------:|
|**Arrow**   |채보 선택, 옵션 선택      |**1, 2**    |스크롤 속도 조절        |**F5**  |채보갱신                  |
|**Enter**   |게임시작, 옵션 확인       |**F2**      |검색                   |**F10** |시스템 설정               |
|**Escape**  |뒤로가기, 게임종료        |**F3**      |코멘트 작성             |**F11** |키 설정                  |
|**Space**   |게임 옵션                |            |                       |        |                         |


# 3. 기능
## 3.1. 변속
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521312-87d64431-838d-4022-8591-de8d8006d738.gif">    
            
      변화하는 BPM에 맞춰 노트의 속도가 함께 변경됩니다.

      노트의 기본속도는 스크롤 속도에 의해 결정되며 여러 BPM이 존재할 때 메인BPM과의 비율만큼 속도가 변경됩니다.
            
## 3.2. 속도 조절
* ### 스크롤 속도
<p align="center">

      노트의 기본속도를 결정하는 수치로 모든 채보는 선택된 채보의 BPM과 관계없이 조절된 스크롤 속도에 의해 일정한 속도로 시작합니다.
      
* ### 사운드 속도 ( 배속 )
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521330-a421424f-7475-4d59-884d-d0f7a7b272a4.gif">
<p align="center"> < 좌측( x0.7 ) 중앙( x1.0 ) 우측( x1.5 ) >
                
      사운드 속도는 음악의 피치와 속도가 조절되며 노트의 타격 타이밍이 함께 조정됩니다.
      
      조절 값에 따라 노트 밀도에 영향을 주어 플레이어 스스로 난이도 조절이 가능합니다.
    
## 3.3. BGA
* ### Video
        
      배경 타입 중 우선적으로 채택되는 타입으로 .mkv, .mp4 등 비디오 파일을 통해 재생됩니다.
                           
* ### Sprite

      이미지 교체를 통해 영상처럼 보여지는 타입입니다.
                    
* ### Image
      
      영상 형식의 데이터가 없을 때 해당 채보의 배경 이미지를 보여줍니다.
      
      모든 배경 데이터가 없을 때 미리 로딩된 기본 이미지를 보여줍니다.

## 3.4. 키음

      # 배경음 영역 - 정의된 데이터에 의해 자동으로 재생되는 사운드로 특정 모드에 의해 제거된 키가 있을 경우 배경음 영역에서 재생됩니다.
                   
      # 키음 영역 - 판정될 노트의 사운드를 레인에 설정하여 키를 입력했을 때 해당 사운드를 재생합니다.

## 3.5. 게임 모드
* ### Random
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/248521388-11f838dd-b02c-4e13-8eb8-f15a3b2b49e4.gif">
<p align="center"> < 좌측( None ) 중앙( Basic ) 우측( Max ) >

      # Mirror - 레인이 좌우반전되어 시작합니다.

      # Basic Random - 모든 레인을 무작위로 섞습니다.
      
      # Half Random - 중앙을 기준으로 절반씩 무작위로 섞습니다. 
      
      # Max Random - 모든 노트를 무작위 레인에 배치합니다.

* ### AutoPlay
      
      모든 판정을 자동으로 처리합니다.
      
* ### NoSlider
      
      롱노트를 일반노트로 변환하여 시작합니다.

* ### NoFail
      
      게임오버가 되지 않습니다.

* ### FixedBPM
     
      메인BPM만 사용하도록 고정하여 변속 기능을 제거합니다.

* ### 7K To 6K
     
      7키 채보의 일부 키를 제거하여 시작합니다.

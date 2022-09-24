
<p align="center"> *** 본 프로젝트는 취업목적만으로 제작한 개인 프로젝트입니다 ***
      
- - -
      
# 1. 소개
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/192090994-a74490b6-86d2-41c1-ac09-f3453efd69c3.gif"> 

      리듬게임 실력을 높이고싶은 6키가 주력키인 사용자를 위해 제작한 건반형 리듬게임 구동기입니다.
      
      실력을 늘릴 수 있는 각종 옵션이나 편의성에 대한 기능을 많이 생각하고 제작했습니다.

# 2. 사용 방법
## 2.1. 파일 목록
      
      파싱 가능한 파일은 .osu 확장자 파일입니다.
      
## 2.2. 채보 추가 방법

      게임설치 경로에 있는 StreamingAssets\\Songs 폴더안의 파일들을 탐색하여 곡 리스트를 작성합니다.
      각종 리소스, 채보 데이터 등이 포함된 폴더를 해당 경로안에 생성하시면 됩니다.
      
      7키 이상의 채보는 6키로 고정되어 일부 키를 제거하고 키음 데이터는 자동재생되도록 변환됩니다.
      
## 2.3. 조작 방법
      
      게임 내에서 마우스는 사용하지 않습니다.
      
      사용자가 직접 게임세팅을 통해 정의한 키를 제외하고 방향키, 엔터, 스페이스바, ESC 키로 대부분의 조작이 가능하며
      예외로 스크롤 속도는 키보드 상단의 1, 2 키를 통해 조절할 수 있습니다.

# 3. 기능
## 3.1. 변속 ( BPM 변화 )
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/192090996-143422c0-63f3-4b4f-ab86-39d1c3b0af73.gif">    
            
      가변적인 BPM에 맞춰 내려오는 노트의 속도도 달라집니다.
      
## 3.2. 속도 조절
### 3.2.1. 스크롤 속도
      
      스크롤 속도 변경을 통해 내려오는 노트의 속도를 조절할 수 있습니다.
      
      추가로 서로 다른 BPM을 가진 채보를 플레이 할 때마다 스크롤 속도를 조절하지 않아도 됩니다.
      해당 채보에서 가장 긴 시간 지속되는 BPM을 기준으로 스크롤 속도를 일정비율 자동으로 조절하여 시작합니다.
      
### 3.2.2. 사운드 속도
<p align="center"> <img src="https://user-images.githubusercontent.com/19517385/192102583-13d6f6a2-4513-43c0-b0ff-0c9246b72707.gif">
<p align="center"> < 왼쪽부터 0.7  1.0  1.5 배속 >
                
      사운드 속도 변경을 통해 사용자가 난이도를 조절할 수 있습니다.
    
      게임 내에서 사운드 속도를 0.1 단위로 0.7 ~ 1.5 까지 조절할 수 있고 값에 따라 노트의 간격도 함께 조정됩니다.
    
## 3.3. BGA
    - **Video**
        
          여러 배경타입이 있을 때 가장 우선적으로 채택되는 타입입니다.
        
          채보 데이터 중 VideoPath 경로의 비디오 파일을 재생합니다.
                
    - **Sprite**
        
          두 번째 우선순위를 가지는 타입입니다.
        
          채보 데이터 중 [Sprites] 하위 목록들을 참조하여 이미지를 한장씩 교체해가며 재생합니다.
        
          중복된 데이터는 로딩하지 않도록 했습니다.
        
    - **Image**
        
          마지막 우선순위를 가지는 타입입니다.
        
          채보 데이터 중 ImagePath 경로의 이미지를 보여줍니다.
    
    위의 3개의 데이터가 모두 없으면 기본 이미지를 로딩합니다.
<br></br>

- **키음**
    - **노트 키음**
        
          해당 라인의 가장 앞에 있는 노트의 키음을 재생합니다.
        
    - **자동 재생 키음**
        
          채보 데이터에 정의된 시간에 맞춰 자동으로 재생됩니다.
        
    
    BGA Sprite 타입과 마찬가지로 중복된 데이터는 로딩하지 않습니다.
<br></br>

- **게임 모드**
    - **Mirror**
        
          6개의 라인을 반전시킵니다. ( 1, 2, 3, 4, 5, 6 → 6, 5, 4, 3, 2, 1 )
        
    - **Basic-Random**
        
          6개의 라인을 무작위로 섞습니다.
        
    - **Half-Random**
        
          왼손, 오른손 기준으로 3라인씩 무작위로 섞습니다.
        
    - **Max-Random**
        
          노트마다 무작위의 라인에 배치합니다.
        
    - **AutoPlay**
        
          자동연주 기능을 설정할 수 있습니다.
        
    - **NoSlider**
        
          롱노트를 일반노트로 변환합니다.

# Carpal Tunnel Syndrome

## 소개

![https://postfiles.pstatic.net/MjAyMjA2MTVfMjgy/MDAxNjU1MjU1Mzc2MTY0.nyRRv5lFwc2ZfZxii-_T3fLIC6xFjcwoLfYe7MVXWQsg.37po0gK7oP86hHMAf7pNYac0lxOFdCRGCsuAIVrOMYwg.GIF.ashi0/Title_GIF.gif?type=w966](https://postfiles.pstatic.net/MjAyMjA2MTVfMjgy/MDAxNjU1MjU1Mzc2MTY0.nyRRv5lFwc2ZfZxii-_T3fLIC6xFjcwoLfYe7MVXWQsg.37po0gK7oP86hHMAf7pNYac0lxOFdCRGCsuAIVrOMYwg.GIF.ashi0/Title_GIF.gif?type=w966)

BMS, Osu Mania! 등 다양한 채보를 6키로 즐기는 것을 목표로 제작한 리듬게임 구동기입니다.

7, 8키 채보는 일부 키를 제거하고 키음 데이터를 자동으로 재생하도록 변환됩니다.

( 계단과 겹계단 패턴을 보존하기 위해 스크래치라고 불리는 키와 마지막 키를 제거했습니다. )

게임 설치 경로에 있는 StreamingAssets\\Songs 폴더안의 파일을 탐색하여 곡 리스트를 작성합니다.

**파일** **목록**

- Osu Mania! ( .osu )
- ~~BMS ( .bms, .bml, .bme … ) 제작 중~~

## 기능

- **변속 ( BPM 변화 )**

![https://postfiles.pstatic.net/MjAyMjA2MTVfMjEz/MDAxNjU1MjU1Mzc1NjI5.Af-fwQaxtIEgNn7khOjZBhUuGjIounOg257ETHPbYbcg.zE0F_j_Vn5iEeyL8QBxk16NPl6qwKcJlXEv3Qzw8E1Eg.GIF.ashi0/BPM_GIF.gif?type=w966](https://postfiles.pstatic.net/MjAyMjA2MTVfMjEz/MDAxNjU1MjU1Mzc1NjI5.Af-fwQaxtIEgNn7khOjZBhUuGjIounOg257ETHPbYbcg.zE0F_j_Vn5iEeyL8QBxk16NPl6qwKcJlXEv3Qzw8E1Eg.GIF.ashi0/BPM_GIF.gif?type=w966)

여러 BPM에 대응하여 내려오는 노트의 속도가 달라집니다.

서로 다른 BPM을 가진 곡을 플레이할 때 일정한 속도를 유지하기위해

해당 곡의 가장 오래 지속되는 BPM을 기준으로 스크롤속도를 일정비율 자동으로 조절합니다.

- **BGA ( Background Animation )**
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
    

- **키음**
    - **노트 키음**
        
        해당 라인의 가장 앞에 있는 노트의 키음을 재생합니다.
        
    - **자동 재생 키음**
        
        채보 데이터에 정의된 시간에 맞춰 자동으로 재생됩니다.
        
    
    BGA Sprite 타입과 마찬가지로 중복된 데이터는 로딩하지 않습니다.
    
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
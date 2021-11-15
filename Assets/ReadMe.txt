# Timing Points 구성
Time, Beat Length, Meter, Sample Set, Sample Index, Volume, Uninherited, Effects
Ex ) 905,312.5,4,2,0,10,1,0

1. Time ( Integer )
타이밍 시작 시간 ( 오디오가 시작된 시간부터 밀리초 단위 )
타이밍의 끝은 다음 타이밍의 시작 시간이며, 다음 타이밍이 없을 경우 변경 안함.

2. Beat Length ( Decimal )
- 첫번째 : 비트 지속시간
- 두번째 : 상속됐을 경우 슬라이더 속도의 승수가 백분율로 표현, -50일때 2배 빨라짐.

3. Meter ( Integer )

4. Sample Set ( Integer )
히트 객체에 대한 기본 샘플 세트 ( 0 = 비트맵 기본값, 1 = 일반, 2 = 소프트, 3 = 드럼 )

5. Sample Index ( Integer )
히트 객체에 대한 사용자 지정 샘플 인덱스 ( 0 = 기본 히트 사운드 )

6. Volume ( Integer )

7. Uninherited ( 0 or 1 )
상속 됐는지 여부

8. Effects ( Integer )
타이밍 포인트에 추가 효과를 추는 비트 플래그



# HitObjects
X, Y, Time, Type, Hit Sound, Object Parameters, Hit Sample
Ex) 42,192,1371,128,0,1528:0:0:0:0:

1,2. X And Y ( Integer )
개체를 어느 픽셀에 배치할지 결정 
기본적으로 Y축은 건들지 않고, X축은 어느 키의 위치에 있을지 결정한다.
노래마다 X축이 다 다른데 정렬이 필요하다. ( 6키 기준 X축 값이 6개로 모든노래가 동일하다. )

3. Time ( Integer )
오디오가 시작된 시점부터 타격할 시간 ( 밀리초 단위 )

4. Type ( Integer )
개체 유형을 나타내는 플래그 값
Osu! Mania 기준으로 1은 일반노트 128이 롱노트 인것 같음

5. HitSound ( Integer )
개체에 적용되는 히트 사운드 플래그 값

6. Object Parameters ( Comma-Separated list )
개체 유형과 관련된 추가 매개 변수 ( 롱노트 일 경우 언제 떼야되는지일듯 )

7. Hit Sample ( Colon-Separated List)
개체가 적중할때 재생되는 사운드 샘플에 대한 정보



# BPM 공식
Timing Points의 2번째 요소 값 = 333.33
1 / 333.33 * 1000 * 60 = 180BPM
Timing이 여러개면 노래 중간에 BPM이 바뀐다는 뜻


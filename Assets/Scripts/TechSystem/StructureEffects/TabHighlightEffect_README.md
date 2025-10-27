# 탭 하이라이트 기능 사용 가이드

## 개요
건물을 처음 업그레이드할 때 특정 탭 버튼이 무지개 색으로 반짝이는 효과를 추가했습니다.

## 생성된 파일들

### 1. TabHighlightEffect.cs
- 경로: `Assets/Scripts/TechSystem/StructureEffects/TabHighlightEffect.cs`
- BaseStructureEffect를 상속받는 스크립터블 오브젝트
- 특정 탭에 하이라이트 효과를 적용

### 2. RainbowButtonEffect.cs
- 경로: `Assets/Scripts/UI/RainbowButtonEffect.cs`
- 버튼에 무지개 색 반짝임 효과를 추가하는 컴포넌트
- Image 컴포넌트가 필요

### 3. TechViewer.cs (수정됨)
- 탭 하이라이트 관리 기능 추가
- 탭 클릭 시 자동으로 효과 비활성화

## 사용 방법

### 1. TabHighlightEffect 스크립터블 오브젝트 생성

1. Unity 에디터에서 프로젝트 창에서 우클릭
2. `Create > Scriptable Objects > Structure Effect > Tab Highlight Effect` 선택
3. 생성된 에셋의 Inspector에서 설정:
   - **Target Tab**: 하이라이트할 탭 선택
     - `Job` (일꾼)
     - `Structure` (건물)
     - `Special` (특수)

### 2. TechData에 효과 연결

건물 업그레이드 시 효과를 발동시키고 싶은 TechData의 StructureEffect 리스트에 생성한 TabHighlightEffect를 추가합니다.

예시:
```
첫 번째 건물 업그레이드 TechData
└─ Structure Effects (List)
   └─ Element 0: TabHighlightEffect (Target Tab: Job)
```

### 3. 자동 동작

- 건물이 업그레이드되면 자동으로 설정한 탭이 무지개 색으로 반짝입니다
- 사용자가 해당 탭을 클릭하면 자동으로 효과가 꺼집니다
- 원래 색상으로 복구됩니다

## RainbowButtonEffect 설정 옵션

Inspector에서 다음 값들을 조정할 수 있습니다:

- **Color Change Speed**: 무지개 색상 변경 속도 (기본: 2)
- **Pulse Speed**: 밝기 변화(펄스) 속도 (기본: 3)
- **Min Alpha**: 최소 투명도 (기본: 0.7)
- **Max Alpha**: 최대 투명도 (기본: 1.0)

## 코드에서 수동 제어

필요한 경우 코드에서 직접 제어할 수 있습니다:

```csharp
// 특정 탭 하이라이트 활성화
TechViewer.instance.ActivateTabHighlight(TechKind.Job);

// 버튼 컴포넌트에서 직접 제어
RainbowButtonEffect effect = button.GetComponent<RainbowButtonEffect>();
effect.ActivateEffect();    // 활성화
effect.DeactivateEffect();  // 비활성화
bool isActive = effect.IsEffectActive();  // 상태 확인
```

## 주의사항

1. TabHighlightEffect는 BaseStructureEffect를 상속받으므로 구조물 완성 시 발동됩니다
2. TechViewer.instance가 null이 아닌지 확인하세요
3. 버튼에는 Image 컴포넌트가 필요합니다
4. 효과는 탭을 클릭할 때 자동으로 꺼집니다

## 구조

```
TabHighlightEffect (ScriptableObject)
    ↓ ApplyTechEffect()
TechViewer.ActivateTabHighlight()
    ↓
RainbowButtonEffect.ActivateEffect()
    ↓ (Update 루프)
무지개 색상 효과 표시
    ↓ (탭 클릭 시)
TechViewer.OnClickTab()
    ↓
RainbowButtonEffect.DeactivateEffect()
    ↓
원래 색상 복구
```

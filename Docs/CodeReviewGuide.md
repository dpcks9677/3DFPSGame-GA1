# Unity C# 코드 리뷰 기준

## 1. 리뷰 원칙

* 리뷰 대상은 `commit.diff`의 `Assets/02.Scripts/**/*.cs` 변경분이다. diff에 없는 파일(`.meta`, 씬, 프리팹, 다른 스크립트)은 확인하지 못하므로, 그에 의존하는 판단은 "확인 필요"로 표시한다.
* 변경된 코드에서 **실제로 발생 가능한 문제**를 우선한다.
* 취향 차이·과도한 리팩터링을 필수 수정처럼 표현하지 않는다.
* 문제마다 **위치 → 원인 → 결과 → 수정안**을 구체적으로 쓰고, 가능하면 짧은 코드 예시를 붙인다.
* 변경과 무관한 기존 문제는 `참고사항`으로 분리한다.
* 확실하지 않은 내용은 단정하지 않고 "확인 필요"로 표시한다.
* 코드 규모·프로젝트 단계에 맞는 수준으로 제안한다. 패턴·추상화를 무조건 적용하지 않는다.
* **도구가 잡는 항목은 반복 지적하지 않는다.** 들여쓰기·중괄호·네이밍은 `.editorconfig` + `dotnet format`(pre-commit)이 검사한다. 리뷰는 도구가 못 잡는 로직·설계·Unity 동작에 집중한다.

## 2. 중요도

| 등급 | 기준 | 조치 |
| --- | --- | --- |
| Critical | 크래시, 데이터 손상, 세이브 파손, 보안 문제 | 즉시 수정 |
| High | 기능 오류, 체감되는 성능 저하 가능성이 높음 | 다음 커밋 전 수정 |
| Medium | 안정성·확장성·유지보수성 측면에서 수정 권장 | 수정 권장, 미루면 이슈로 남김 |
| Low | 가독성, 단순화 등 선택 개선 | 선택 |

## 3. 프로젝트 컨벤션 (`.editorconfig` 기준, `Assets/02.Scripts/`)

* 들여쓰기 공백 4칸, 여는 중괄호는 새 줄(Allman).
* 중괄호는 여러 줄일 때만 강제. 단일 행 조기 리턴은 생략 허용.
* private 필드 `_camelCase`, private `const` / `static readonly`는 `PascalCase`.
* 그 외 public 멤버·타입·메서드는 `PascalCase`.

## 4. 가독성

### 이름
* 이름만으로 역할을 알 수 있는가? `data`, `value`, `temp`, `manager` 같은 넓은 이름을 남용하지 않는가?
* `bool`은 상태로 읽히는가? (`_isDead`, `CanAttack`, `HasTarget`)
* 메서드는 동사로 동작을 표현하는가?

### 매직 넘버
의미가 드러나지 않는 숫자는 상수 또는 Inspector 값으로 분리한다.

```csharp
// Before
if (currentLevel >= 10) damage *= 1.5f;

// 고정 규칙 → 상수
private const int SpecialLevel = 10;
private const float SpecialDamageMultiplier = 1.5f;

// 기획 조정 대상 → Inspector
[SerializeField] private int _specialLevel = 10;
[SerializeField] private float _specialDamageMultiplier = 1.5f;
```

* 예외: 초기화·비교의 `0`, 증감·기본 배율의 `1`, 검색 실패 `-1`, 반복문 시작/증감값. 단, 의미가 모호하면 이것도 분리한다.
* 우선 확인 대상: 체력·공격력, 이동·회전 속도, 쿨타임·지속시간, 경험치·레벨, 확률·가중치, 최대 개수·제한 횟수, 데미지 배율, **레이어 번호·씬 인덱스·애니메이션 상태**(→ `LayerMask`, 씬 이름/상수, `Animator.StringToHash`).

## 5. 설계

> 모든 코드에 SOLID·인터페이스·패턴을 강제하지 않는다. 작고 변경 가능성이 낮은 코드는 단순한 구조가 낫다.

### 묻지 말고 시켜라
```csharp
// 외부가 상태를 판단 → 결합도 증가
if (player.Health <= 0) player.Die();

// 객체에 행동을 요청 → 체력 감소·사망 판단·이벤트 발생은 Player 내부 책임
player.TakeDamage(damage);
```
* 외부에서 상태를 반복 조회해 판단하거나, 같은 판단이 여러 클래스에 중복되는가?
* 프로퍼티로 내부 구현이 과도하게 노출되는가?
* 단, UI 표시·디버깅·저장/직렬화용 단순 조회까지 바꾸라고 요구하지 않는다.

### 응집도·결합도
* 관련 없는 기능이 한 클래스에 섞여 있는가? 관련 데이터와 동작은 함께 있는가?
* 한 클래스가 너무 많은 클래스를 직접 참조하거나, 구체 구현에 의존하는가?
* 작은 변경이 여러 클래스 수정으로 번지는가?
* Singleton·전역 상태에 불필요하게 의존하는가? 순환 참조가 있는가?

### SOLID 체크
| 원칙 | 확인할 것 | 과잉 적용 주의 |
| --- | --- | --- |
| SRP | 변경 이유가 하나인가? `Player`가 입력·이동·전투·체력·애니메이션·사운드·저장·UI를 모두 처리하면 `PlayerInput / Movement / Combat / Health / Animation` 분리 검토 | 메서드 수만으로 쪼개지 않는다. 밀접한 상태를 과하게 나누면 오히려 결합도 증가 |
| OCP | 타입 추가 때마다 `if / switch (enemyType)`을 계속 고치는가? → 상속·인터페이스·전략·컴포지션 | 종류가 적고 확장 계획이 없으면 조건문 유지 |
| LSP | 자식이 부모 의미를 바꾸거나, `NotSupportedException`을 던지거나, 호출부가 타입 검사를 하게 만드는가? → 컴포지션·별도 인터페이스 | — |
| ISP | `ICharacter { Move; Attack; Fly; Swim; }`처럼 안 쓰는 기능 구현을 강요하는가? → `IMovable`, `IAttackable` 등 역할별 분리 | 메서드 하나짜리 인터페이스 남발 금지 |
| DIP | `new KeyboardInput()`처럼 상위 로직이 구체 구현에 묶였는가? → `IPlayerInput` 등 추상에 의존 | 교체 대상이 실제로 있을 때만 |

Unity에서의 의존성 전달 수단: Inspector 참조, 메서드 주입, 인터페이스 구현 컴포넌트, ScriptableObject(데이터·설정 분리), 이벤트(직접 참조 감소).

## 6. C# 체크리스트

* **Null / 범위**: NullReferenceException, 컬렉션 범위 초과, 빈 컬렉션 처리.
* **이벤트 / 리소스**: 구독 후 해제, `IDisposable` 정리.
* **할당 / 성능**: 반복되는 박싱·문자열 생성, 핫패스의 LINQ·클로저 할당.
* **캡슐화**: 불필요한 public 필드, 과도한 setter 노출. → `[SerializeField] private` 사용.
* **예외 / 비동기**: 실패 경로 처리. `async void`는 이벤트 핸들러 외 금지, 예외와 취소(`CancellationToken`) 처리.
* **중복**: 같은 코드·조건 판단이 여러 곳에 있는가?

## 7. Unity 체크리스트

### 생명주기
* `Awake`(자기 초기화) / `OnEnable`(구독) / `Start`(타 객체 참조) 역할이 구분되는가? 실행 순서에 과도하게 의존하지 않는가?
* `OnEnable`에서 구독한 이벤트·Input Action 콜백을 `OnDisable`에서 해제하는가?
* Coroutine·`Invoke`·async 작업이 오브젝트 파괴 후에도 남는가? (`destroyCancellationToken`, `StopCoroutine`)

### 성능
* `Update`/`FixedUpdate`에서 매 프레임 할당(`new`, 문자열 연결, LINQ)이 있는가?
* `Find`, `FindObjectOfType`, `GetComponent`, `Camera.main`을 매 프레임 호출하는가? → 캐싱.
* `Instantiate`/`Destroy`가 잦은가? (투사체·이펙트·UI 항목 등) → 오브젝트 풀링.
* 다중 Raycast·Overlap에서 `NonAlloc` 버전을 고려했는가?
* `tag ==` 대신 `CompareTag`, 애니메이터 파라미터는 `Animator.StringToHash`로 캐싱했는가?

### 물리 / 시간 / 입력
* 물리 처리는 `FixedUpdate`, 시간 기반 처리는 `Time.deltaTime`/`fixedDeltaTime`을 쓰는가? 프레임레이트에 따라 결과가 달라지지 않는가?
* 마우스·포인터 델타처럼 이미 프레임 이동량인 입력에 `deltaTime`을 중복으로 곱하지 않았는가?
* Raycast·Overlap·충돌 판정에 `LayerMask`와 거리를 지정했는가? 자기 자신 콜라이더를 감지하지 않는가?
* 빠르게 움직이는 오브젝트가 충돌을 놓치지 않는가? (Continuous 충돌 감지 또는 Raycast 보정)
* `Time.timeScale = 0`(일시정지)에서 의도대로 동작하는가? (`unscaledDeltaTime` 필요 여부)
* Input System 액션을 `Enable`/`Disable`하는가? 일시정지·UI 상태에서 게임 입력을 막는가?

### 직렬화 / 에셋
* 파괴된 `UnityEngine.Object`에 `?.` / `??`를 쓰지 않았는가? (Unity의 오버로드된 null 비교를 우회함)
* 필수 Inspector 참조를 검증하는가? (`OnValidate`, `Awake`에서 null 체크·로그)
* 직렬화 필드 이름을 바꿨다면 `[FormerlySerializedAs]`로 기존 값을 보존했는가?
* 런타임 데이터가 원본 ScriptableObject를 수정하지 않는가? (에디터에서 값이 영구 변경됨)

## 8. 리뷰 결과 형식

### 총평
변경 목적과 전체 평가를 2~3문장으로 쓴다. Critical/High 개수를 함께 적는다.

### 발견된 문제
문제마다 아래 형식으로 쓴다.

```text
[High] Assets/02.Scripts/PlayerCombat.cs:42 — 한 줄 요약
- 원인:
- 결과:
- 수정안: (필요하면 코드)
```

### 설계 개선 제안 (구조적 문제가 명확할 때만)
패턴 이름만 던지지 않고 다음을 설명한다.

1. **현재 문제**: 어떤 책임·의존성 때문에 변경이 어려운가
2. **개선 구조**: 분리·추가할 클래스, 컴포넌트, 인터페이스와 각각의 책임
3. **상호작용**: 메서드·인터페이스·이벤트 중 무엇으로 연결하는가
4. **비용**: 얻는 장점과 클래스 수 증가·추상화 비용

### 참고사항
변경과 무관한 기존 문제를 쓴다. 없으면 생략한다.

### 잘된 점
실제로 잘된 부분이 있을 때만 짧게 쓴다.

> 문제를 찾지 못했다면 억지로 만들지 말고, 확인한 범위와 "문제 없음"을 명시한다.

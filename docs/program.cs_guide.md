### `Program.cs` 가이드

`Program.cs` 파일은 이 애플리케이션의 메인 진입점 역할을 합니다. Cocona 라이브러리를 사용하여 명령줄 인수를 처리하고, GitHub API를 통해 저장소 데이터를 수집하며, 결과를 출력합니다.

#### 사용법

애플리케이션을 실행할 때 다음과 같은 인자 및 옵션을 사용할 수 있습니다.

```bash
dotnet run -- <owner> <repo> [options]
```

* **`<owner>`**: GitHub 저장소의 소유자 이름입니다.
* **`<repo>`**: GitHub 저장소의 이름입니다.

**예시:**

```bash
dotnet run -- octokit/octokit.net
```

#### 옵션

* `-v`, `--verbose`
    * 자세한 로그 출력을 활성화합니다.
    * **예시:** `dotnet run -- octokit/octokit.net -v`
* `-t`, `--token <token>`
    * GitHub Personal Access Token(PAT)을 입력합니다. 이 토큰은 GitHub API 호출 시 인증에 사용됩니다.
    * **예시:** `dotnet run -- octokit/octokit.net -t your_github_token`
* `-o`, `--output <directory>`
    * 출력 디렉토리 경로를 지정합니다. 지정하지 않으면 기본값으로 `output` 디렉토리에 저장됩니다.
    * **예시:** `dotnet run -- octokit/octokit.net -o results`
* `-f`, `--format <format>`
    * 출력 형식을 지정합니다. 쉼표로 구분하여 여러 형식을 지정할 수 있습니다 (예: `table,json`). 지정하지 않으면 기본값으로 `table` 형식이 사용됩니다.
    * **예시:** `dotnet run -- octokit/octokit.net -f json,csv`

#### 기능

`Program.cs`는 다음 단계를 수행합니다:

1.  **더미 데이터 로드 확인**: 개발 및 테스트 목적으로 정의된 더미 데이터(`DummyData.repo1Activities`, `DummyData.repo2Activities`)가 올바르게 로드되는지 확인합니다.
2.  **인자 유효성 검사**: `owner`와 `repo` 두 개의 저장소 인자가 제공되었는지 확인합니다.
3.  **GitHub 저장소 정보 표시**: `Octokit` 라이브러리를 사용하여 주어진 저장소의 이름, 전체 이름, 설명, 별표 수, 포크 수, 열린 이슈 수, 언어, URL 등의 기본 정보를 가져와 출력합니다.
4.  **데이터 수집**: `RepoDataCollector` 클래스를 사용하여 GitHub 저장소에서 필요한 데이터를 수집합니다. 이 단계에서는 주로 Pull Request와 Issue의 라벨 통계를 분석합니다.
5.  **오류 처리**: API 호출 한도 초과, 인증 실패, 저장소를 찾을 수 없는 경우 등 발생할 수 있는 다양한 오류를 처리하고 적절한 메시지를 출력합니다.


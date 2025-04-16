# reposcore-cs
A CLI for scoring student participation in an open-source class repo, implemented in C#.

## 참여조건
해당 repository는 프로젝트 참여 점수 10점 미만인 학생만 참여할 수 있습니다

## 예제 코드

아래는 본 프로젝트에 사용된 Cocona 기반 CLI 코드입니다.  
기본적인 명령행 인자 처리와 `--verbose` 옵션을 실험하는 용도로 작성되었습니다.

```csharp
using Cocona;

CoconaApp.Run(([Argument] string[] repository, bool verbose) =>
{
    Console.WriteLine($"Repository: {String.Join("\n ", repository)}");
    if (verbose)
    {
        Console.WriteLine("Verbose mode is enabled.");
    }
});
```

## 사용 방법

아래 명령어를 통해 CLI를 실행할 수 있습니다.

```bash
dotnet --version
dotnet run -- repo1 repo2
dotnet run -- repo1 repo2 --verbose
```

## 실행 예시

```bash
$ dotnet run -- repo1 repo2
Repository: repo1
 repo2

$ dotnet run -- repo1 repo2 --verbose
Repository: repo1
 repo2
Verbose mode is enabled.

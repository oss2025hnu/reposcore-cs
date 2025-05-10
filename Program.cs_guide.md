
## Program.cs Guide

안내하고 있는 방법과 같은 방식으로 다른 `.cs` 파일에도 동일하게 적용할 수 있습니다.

## Greeter.cs(예시 코드)
```csharp

public class Greeter

{

    public void SayHello(string name)

    {

        Console.WriteLine($"Hello, {name}!");

    }

}
```

## Greeter.cs의 기능

`Greeter.cs`는 `SayHello(string name)` 메서드를 통해 **인삿말을 출력하는 간단한 코드**입니다.


## 사용 방법

`Program.cs`에서 사용하려면 `CoconaApp.Run` 내부의 실행 로직 안에 작성해야 합니다.

```csharp
using Cocona;
using System;
using Octokit;

CoconaApp.Run((
    [Argument] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option("o|output", Description = "출력 디렉토리 경로를 지정합니다.")] string? output,
    [Option("f|format", Description = "출력 형식을 지정합니다.")] string? format
) =>

{
    // 여기서 Greeter 인스턴스를 사용(SayHello("GitHub User");을 출력해줌)
    var greeter = new Greeter();
    greeter.SayHello("GitHub User");

     // ...(아래 코드 동일하게)
});
```

이와 같은 형식으로 작성해주시면 됩니다.

## 주의

단, Greeter.cs 파일이 같은 프로젝트에 추가되어 있어야 합니다.

Greeter 클래스는 public이고 namespace 없이 전역 범위에 있어야 Program.cs에서 바로 쓸 수 있습니다.

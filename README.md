# single-instance-app (.NET Core)

Handles the scenario when you only want one instance of the application running on the local user logon. 
There is also the option to send commands to the other instance using name pipes communication.

### Code Example
```C#
var instanceService = new SingleInstanceService();
if (!await instanceService.Start())
{
    await Console.Out.WriteLineAsync("There is all ready another instance of this application running");
    await instanceService.SignalFirstInstance(new List<string>() {"Hello from app1"});
}
else
{
    await Console.Out.WriteLineAsync("This is the first instance of this application running");
}

```



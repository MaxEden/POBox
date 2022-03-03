# POBox 

POBox is a temporary container for produces consumer scenarios.
POBox is a solution for storing results of long working services with a big execution time or long waiting queues.
POBox allows you to create a strongly typed parcel with a password, mandatory expiration time and a disposing method for correct release of the parcel resources.
POBox is thread safe and can be used without additional checks.

Put to storage
```c#

int resultsToken = Storage.Post(new Results()
{
    FileName = resultFileName,
    OneFileSourceText = oneFileText,
    Files = files,
    InpupPath = targetDllPath,
    OutputPath = outputPath,
}, 
lifeTime: TimeSpan.FromMinutes(15),
password: resultFileName,
disposer: p =>
{
    File.Delete(p.InpupPath);
    File.Delete(p.OutputPath);
});
```

Retrieve from storage
```c#
if(Storage.Get<Results>(fileName, out var parcel, consume: false))
{
    p.Result = parcel.FileName;
}
else
{
    p.Error = "file not found";
}
```





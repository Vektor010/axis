using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string p1 = @"C:\Users\User\Documents\win-tweaker\Tweaker.csproj";
        string t1 = File.ReadAllText(p1, Encoding.UTF8);
        if (!t1.Contains("MmcssTaskService.cs"))
        {
            t1 = t1.Replace("<Compile Include=\"src\\Engine\\GpuHwschService.cs\" />",
                            "<Compile Include=\"src\\Engine\\GpuHwschService.cs\" />\r\n    <Compile Include=\"src\\Engine\\MmcssTaskService.cs\" />");
            File.WriteAllText(p1, t1, Encoding.UTF8);
            Console.WriteLine("Tweaker.csproj updated");
        }
        else
        {
            Console.WriteLine("Tweaker.csproj already contains MmcssTaskService");
        }

        string p2 = @"C:\Users\User\Documents\win-tweaker\Tests\Tests.csproj";
        string t2 = File.ReadAllText(p2, Encoding.UTF8);
        if (!t2.Contains("MmcssTaskService.cs"))
        {
            t2 = t2.Replace("<Compile Include=\"..\\src\\Engine\\GpuHwschService.cs\" />",
                            "<Compile Include=\"..\\src\\Engine\\GpuHwschService.cs\" />\r\n    <Compile Include=\"..\\src\\Engine\\MmcssTaskService.cs\" />");
            File.WriteAllText(p2, t2, Encoding.UTF8);
            Console.WriteLine("Tests.csproj updated");
        }
        else
        {
            Console.WriteLine("Tests.csproj already contains MmcssTaskService");
        }
    }
}

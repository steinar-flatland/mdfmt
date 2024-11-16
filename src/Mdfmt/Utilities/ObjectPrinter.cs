using System;
using System.Reflection;

namespace Mdfmt.Utilities;

public static class ObjectPrinter
{
    public static void PrintProperties(object obj, string prepend = "")
    {
        if (obj == null)
            Console.WriteLine("null");

        Type type = obj.GetType();

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            object value = property.GetValue(obj);
            Console.WriteLine($"{prepend}{property.Name}: {value}");
        }
    }
}

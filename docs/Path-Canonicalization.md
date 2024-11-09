# Path Canonicalization

The module `Mdfmt.Utilities.PathUtils.cs` has a method,

```C#
public static string Canonicalize(string cpath, string rpath) { }
```

It is not used now, but it could become important if we decide to do something related to relative paths between files within the Mdfmt context, where one file is referring to a second file through a relative path expressed from the first file's point of view, and canonicalization is needed, e.g. to make a standard key to access a data structure.

Canonicalize has the following signature:

```C#
  string Canonicalize(cpath, rpath);
```
  
- Canonicalize is being called in a directory that is referred to as "the root" in the discussion below.  The root defines everything Mdfmt can know.  To make a comparison with another tool, this root is similar to the Docker context in the Docker world.  We could call this root the "Mdfmt context."
- Conceptually, the cpath and rpath parameters (which will be explained more deeply in a moment) both refer to files in the root  (but in different ways), each file either directly contained in the root directory or in a subfolder of the root.
- Here is an example of a directory structure for discussion.  Capital letters represent directories, and lowercase letters represent files.
  - A is the root.
  - A contains B, C, a.
  - B contains D, E, b.
  - C contains F, G, c.
  - D contains d.
  - E contains H, e.
  - F contains I, f.
  - G contains g.
  - H contains h.
  - I contains J, i.
  - J contains j.

Every file has a canonical path, or cpath:

- ./a
- ./B/b
- ./C/c
- ./B/D/d
- ./B/E/e
- ./C/F/f
- ./C/G/g
- ./B/E/H/h
- ./C/F/I/i
- ./C/F/I/J/j

Now, about the arguments to Canonicalize():

- `cpath` is simply the canonical path of one of the files in root.
- `rpath` expresses the location of some file in root, as a path that is relative to the directory that contains the cpath file.

Here are examples of expressing the locations of files in root as a path that is relative to the directory that contains the cpath file.  Each example is expressed as a 3 tuple comprising (cpath, target file name, rpath):

- (./B/D/d, d, ./d)
- (./B/D/d, h, ../E/H/h)
- (./a, c, ./C/c)
- (./a, g, ./C/G/g)
- (./B/E/H/h, j, ../../../C/F/I/J/j)
- (./C/F/f, j, ./I/J/j)
- (./C/F/I/i, g, ../../G/g)

Summarizing what `CanonicalPath(cpath, rpath)` does:  It takes a `cpath` of a file in root, and an `rpath` to a file in root that is expressed relative to the directory that contains the `cpath` file.  It returns a canonical path that references the same file as the `rpath`.

In short, `CanonicalPath(cpath, rpath)` converts the `rpath` to a canonical path, based on the `cpath` argument for context.

Here are some examples of running the function, each run expressed as a 3-tuple (cpath, rpath, return value):

- (./B/D/d, ./d, ./B/D/d)
- (./B/D/d, ../E/H/h, ./B/E/H/h)
- (./a, ./C/c, ./C/c)
- (./a, ./C/G/g, ./C/G/g)
- (./B/E/H/h, ../../../C/F/I/J/j, ./C/F/I/J/j)
- (./C/F/f, ./I/J/j, ./C/F/I/J/j)
- (./C/F/I/i, ../../G/g, ./C/G/g)

Here are some sample C# code fragments that could be converted into a unit test of the `PathUtils.Canonicalize(string cpath, string rpath)` method:

```C#
    private static void TestCanonicalize(string cpath, string rpath, string expected)
    {
        string result = PathUtils.Canonicalize(cpath, rpath);
        Console.WriteLine($"({cpath}, {rpath}, {expected}, {result})   {(result == expected ? "OK" : "ERROR")}");
    }

    public static void Main(string[] args)
    {
        TestCanonicalize("./B/D/d", "./d", "./B/D/d");
        TestCanonicalize("./B/D/d", "../E/H/h", "./B/E/H/h");
        TestCanonicalize("./a", "./C/c", "./C/c");
        TestCanonicalize("./a", "./C/G/g", "./C/G/g");
        TestCanonicalize("./B/E/H/h", "../../../C/F/I/J/j", "./C/F/I/J/j");
        TestCanonicalize("./C/F/f", "./I/J/j", "./C/F/I/J/j");
        TestCanonicalize("./C/F/I/i", "../../G/g", "./C/G/g");
        TestCanonicalize("./C/F/I/i", "../../../G/g", "./G/g");
        TestCanonicalize("./C/F/I/i", "../../../../G/g", null);
        TestCanonicalize("./C/F/I/i", "../../../../../G/g", null);
        TestCanonicalize("./C/F/I/i", "../../../../../../G/g", null);
        TestCanonicalize("./C/F/f", "./I/J/X/Y/Z/x", "./C/F/I/J/X/Y/Z/x");
        TestCanonicalize("./C/F/f", "../../B/E/H/../../D/../../C/G/g", "./C/G/g");
        TestCanonicalize("./C/F/f", "../../B/E/H/../../D/../../../C/G/g", null);
    }

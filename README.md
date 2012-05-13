AxScript3
=========

AxScript3 (or axScriptX) is a scripting language written in c# that utilises the use of reflection heavily.

It is a work in progress, but currently supports quite a few key features:

### Working C# bindings.

Easily add in static

	// C#
	public static class Bar
	{
		public static function Test(int a)
		{
			Console.WriteLine("Testing: {0}", a);
		}
	}
	
	public class Foo
	{
		public int biz = 30;
		
		public void Test(int inp)
		{
			biz = inp;
			Console.WriteLine("Testing: {0}", biz);
		}
	}
	
	AxInterpreter AxInt = new AxInterpreter();
	Foo fooInstance = new Foo();
	// Parameters: (string AxFuncName, string NetFuncName, Type OwnerClass)
	AxInt.RegisterStaticFunction("test", "Test", Bar.getType());
	
	// Parameters: (string AxFuncName, NetFunction function)
	AxInt.RegisterFunction("settest", new NetFunction(fooInstance.getType().getMethod("Test"), fooInstance));
	
### Dynamic variables

	// AxScript
	(set "var" 20)
	(print var)
	(set "var" "Hi")
	(print var)
	
### Function declarations

	// AxScript
	~(test: a | (print "Test: " a))
	~(main: args |
		(
			~[EntryPoint]
			(test 40)
			(test 23.234234)
		)
	)
	
### Local Variables

	// AxScript
	(set "var" 20)
	(print var) //global
	(lset "var" 20)
	(print var) //local overrides global lookup
	(print &var) //force global lookup
	(lset "var2" 200)
	(print &var2) //throws error, no global named "var2"
	
### Dynamic parameter sizes

	// AxScript
	(print "a" "b" "c")
	(print "a")
	(set "array" 1 2 3 4 5 6)
	(print "Second element: " array[2]) //prints "Second Element: 3"
	
	//print out all args put in.
	~(test: a ... | (print a) (for @args { arg | (print arg) }))
	
### Easy array creation and manipulation

	// AxScript
	(set "array" 1 2 3)
	(insert array 4 5 6)
	(set "innerArray" 7 8 9)
	(set "array" array innerArray 10 11 12 13)
	(print+ array) //recursively prints all elements.
	(print (length array))
	(removeAt array 2)
	(print+ array)
	(print (length array))
	
### Loops and enumerations

	// AxScript
	(set "enum" [0, 10, 20, 30])
	(set "enum2" [0..31 | 3])
	(print+ enum)
	(print+ enum2)
	(set "total" 0)
	(for enum2 { i |
		(print i)
		(add total i)
	})
	(print "Total: " total)

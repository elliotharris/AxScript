AxScript3
=========

AxScript3 (or axScriptX) is a scripting language written in c# that utilises the use of reflection heavily.

It is a work in progress, but currently supports quite a few key features:

### Working C# bindings.

Easily add in functions.

	// C# --> Compiles to Test.dll
	public static class Bar
	{
        [AxExport("Bar", "Bang field in Bar class")]
        public static int bang = 20;

        [AxExport("BarTest", "Test function in Bar class")]
		public static function Test(int a)
		{
			Console.WriteLine("Testing: {0}", a+bang);
            bang++;
		}
	}
	
    [AxClass("Foo", "Class for doing stuff!")]
	public class Foo
	{
		public int biz;

        public Foo(int a)
        {
            biz = a;
        }

		public void Test(int inp)
		{
			Console.WriteLine("Testing: {0}", inp+biz);
            biz++;
		}
	}
	
Then in AxScript they can be accessed by doing a '#module' compiler statement that imports that module.
Modules are just .NET DLLs with Attributes marking classes, methods, properties, fields, etc.

    // AxScript

    #module "Test.dll"
    ~(main: args |
        (
            ~[EntryPoint]
            (set ^fooTest (Foo 20))     //Create new Foo
            (foo:Test 10)               //Foo.Test(10);
            (BarTest 10)                //Bar.Test(10);
            (printl foo.biz)            //Console.WriteLine(Foo.biz);
            (printl Bar)            //Console.WriteLine(Bar.bang);
        )
    )
	
### Dynamic variables

	// AxScript
	(set ^var 20)
	(print var)
	(set ^var "Hi")
	(print var)
	
### Function declarations

	// AxScript
	~(test: a | (printl "Test: " a))
	~(main: args |
		(
			~[EntryPoint]
			(test 40)
			(test 23.234234)
		)
	)
	
### Local Variables

	// AxScript
	(set ^var 20)
	(printl var) //global
	(lset ^var 20)
	(printl var) //local overrides global lookup
	(printl $var) //force global lookup
	(lset ^var2 200)
	(printl $var2) //throws error, no global named "var2"
	
### Dynamic parameter sizes

	// AxScript
	(printl "{0}, {1}, {2}" "a" "b" "c")
	(printl "a")
	(set ^array 1 2 3 4 5 6)
	(printl "Second element: {0}" array[2]) //prints "Second Element: 3"
	
	//print out all args put in.
	~(test: a ... | (printl a) (for @args { arg | (printl arg) }))
	
### Easy array creation and manipulation

	// AxScript
	(set ^array 1 2 3)
	(insert array 4 5 6)
	(set ^innerArray 7 8 9)
	(set ^array array innerArray 10 11 12 13)
	(printarr array) //recursively prints all elements.
	(printl (length array))
	(removeAt array 2)
	(printarr array)
	(printl (length array))
	
### Loops and enumerations

	// AxScript
	(set ^enum [0, 10, 20, 30])
	(set ^enum2 [0..31 | 3])
	(printarr enum)
	(printarr enum2)
	(set ^total 0)
	(for enum2 { i |
		(printl i)
		(set ^total (add total i))
	})
	(printl "Total: " total)
	
### Function Pointers
	
	~(sum_function: i | 
		(set ^total (add total i))
	)
	
	~(main:
		~[EntryPoint]
		
		(set ^total 0)
		(for [0..10] *sum_function)
		(printl total)
	)

// See https://aka.ms/new-console-template for more information
using MyPeachNetTest;
using org.breezee.MyPeachNet;
using System.Reflection;

//Console.WriteLine("Hello, World!");

SelectTest selectTest = new SelectTest();
selectTest.Select();
//selectTest.WithSelect();
//selectTest.UnionSelect();

Select2Test select2Test = new Select2Test();
//select2Test.Select();
//select2Test.Select2();

InsertTest insertTest = new InsertTest();
//insertTest.Insert();
//insertTest.InsertSelect();

UpdateTest updateTest = new UpdateTest();
//updateTest.Update();

DeleteTest deleteTest = new DeleteTest();
//deleteTest.Delete();



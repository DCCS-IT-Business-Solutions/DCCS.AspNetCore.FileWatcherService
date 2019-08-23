using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;

namespace DCCS.AspNetCore.FileWatcherService.Tests
{
    [TestClass]
    public class ServiceTest
    {
        string _testDirectory;
        [TestInitialize]
        public void Initialize()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }


        [TestMethod]
        public void NewFileTest()
        {
            var settings = new FileWatchSetting();
            settings.Directory = _testDirectory;
            settings.DelayInMS = 100;

            FileWatcherEventArgs args = null;
            int events = 0;
            using (var watch = new FileWatch(settings))
            {
                watch.StartWatching();
                watch.Changed += (o, a) =>
                {
                    args = a;
                    events++;
                };
                string fileName = Path.Combine(_testDirectory, "Test.txt");
                File.WriteAllText(fileName, "Test");
                Thread.Sleep(50);
                Assert.AreEqual(0, events);
                Thread.Sleep(80);
                Assert.AreEqual(1, events);
                Assert.AreEqual(1, args.NewFiles.Length);
                Assert.AreEqual(fileName, args.NewFiles[0]);
                Assert.AreEqual(0, args.ChangedFiles.Length);
                Assert.AreEqual(0, args.DeletedFiles.Length);
                Thread.Sleep(200);
                Assert.AreEqual(1, events);
            };
        }


        [TestMethod]
        public void NewAndChangeFileTest()
        {
            var settings = new FileWatchSetting();
            settings.Directory = _testDirectory;
            settings.DelayInMS = 100;

            FileWatcherEventArgs args = null;
            int events = 0;
            using (var watch = new FileWatch(settings))
            {
                watch.StartWatching();
                watch.Changed += (o, a) =>
                {
                    args = a;
                    events++;
                };
                string fileName = Path.Combine(_testDirectory, "Test.txt");
                File.WriteAllText(fileName, "Test");
                Thread.Sleep(20);
                File.AppendAllText(fileName, "Test");
                Thread.Sleep(30);
                Assert.AreEqual(0, events);
                Thread.Sleep(80);
                Assert.AreEqual(1, events);
                Assert.AreEqual(1, args.NewFiles.Length);
                Assert.AreEqual(fileName, args.NewFiles[0]);
                Assert.AreEqual(0, args.ChangedFiles.Length);
                Assert.AreEqual(0, args.DeletedFiles.Length);
                Thread.Sleep(200);
                Assert.AreEqual(1, events);
            };
        }

        [TestMethod]
        public void NewAndChangeDeleteFileTest()
        {
            var settings = new FileWatchSetting();
            settings.Directory = _testDirectory;
            settings.DelayInMS = 100;

            FileWatcherEventArgs args = null;
            int events = 0;
            using (var watch = new FileWatch(settings))
            {
                watch.StartWatching();
                watch.Changed += (o, a) =>
                {
                    args = a;
                    events++;
                };
                string fileName = Path.Combine(_testDirectory, "Test.txt");
                File.WriteAllText(fileName, "Test");
                Thread.Sleep(20);
                File.AppendAllText(fileName, "Test");
                Thread.Sleep(20);
                File.Delete(fileName);
                Thread.Sleep(20);
                Assert.AreEqual(0, events);
                Thread.Sleep(100);
                Assert.AreEqual(0, events);
            };
        }

        [TestMethod]
        public void RenameFileTest()
        {
            string fileName = Path.Combine(_testDirectory, "Test.txt");
            File.WriteAllText(fileName, "Test");

            var settings = new FileWatchSetting();
            settings.Directory = _testDirectory;
            settings.DelayInMS = 100;

            FileWatcherEventArgs args = null;
            int events = 0;
            using (var watch = new FileWatch(settings))
            {
                watch.StartWatching();
                watch.Changed += (o, a) =>
                {
                    args = a;
                    events++;
                };
                string newFileName = Path.Combine(_testDirectory, "TestNew.txt");
                File.Move(fileName, newFileName);
                Thread.Sleep(50);
                Assert.AreEqual(0, events);
                Thread.Sleep(80);
                Assert.AreEqual(1, events);
                Assert.AreEqual(1, args.NewFiles.Length);
                Assert.AreEqual(newFileName, args.NewFiles[0]);
                Assert.AreEqual(0, args.ChangedFiles.Length);
                Assert.AreEqual(1, args.DeletedFiles.Length);
                Assert.AreEqual(fileName, args.DeletedFiles[0]);

                Thread.Sleep(200);
                Assert.AreEqual(1, events);
            };
        }

        [TestMethod]
        public void DeleteFileTest()
        {
            string fileName = Path.Combine(_testDirectory, "Test.txt");
            File.WriteAllText(fileName, "Test");

            var settings = new FileWatchSetting();
            settings.Directory = _testDirectory;
            settings.DelayInMS = 100;

            FileWatcherEventArgs args = null;
            int events = 0;
            using (var watch = new FileWatch(settings))
            {
                watch.StartWatching();
                watch.Changed += (o, a) =>
                {
                    args = a;
                    events++;
                };
                File.Delete(fileName);
                Thread.Sleep(50);
                Assert.AreEqual(0, events);
                Thread.Sleep(80);
                Assert.AreEqual(1, events);
                Assert.AreEqual(0, args.NewFiles.Length);
                Assert.AreEqual(0, args.ChangedFiles.Length);
                Assert.AreEqual(1, args.DeletedFiles.Length);
                Assert.AreEqual(fileName, args.DeletedFiles[0]);

                Thread.Sleep(200);
                Assert.AreEqual(1, events);
            };
        }


        [TestMethod]
        public void MultipleChangesWhileDelayTimeTest()
        {
            string fileName1 = Path.Combine(_testDirectory, "Test1.txt");
            File.WriteAllText(fileName1, "Test");
            string fileName2 = Path.Combine(_testDirectory, "Test2.txt");
            File.WriteAllText(fileName2, "Test");

            var settings = new FileWatchSetting();
            settings.Directory = _testDirectory;
            settings.DelayInMS = 100;

            FileWatcherEventArgs args = null;
            int events = 0;
            using (var watch = new FileWatch(settings))
            {
                watch.StartWatching();
                watch.Changed += (o, a) =>
                {
                    args = a;
                    events++;
                };
                string fileName3 = Path.Combine(_testDirectory, "Test3.txt");
                File.WriteAllText(fileName3, "Test");
                string fileName4 = Path.Combine(_testDirectory, "Test4.txt");
                File.WriteAllText(fileName4, "Test");
                Thread.Sleep(50);
                Assert.AreEqual(0, events);
                Thread.Sleep(80);
                Assert.AreEqual(1, events);
                Assert.AreEqual(2, args.NewFiles.Length);
                Assert.AreEqual(fileName3, args.NewFiles[0]);
                Assert.AreEqual(fileName4, args.NewFiles[1]);
                Assert.AreEqual(0, args.ChangedFiles.Length);
                Assert.AreEqual(0, args.DeletedFiles.Length);
                Thread.Sleep(200);
                Assert.AreEqual(1, events);
                File.Delete(fileName1);
                File.Delete(fileName2);
                File.AppendAllText(fileName3, "Test");
                File.AppendAllText(fileName4, "Test");
                string fileName5 = Path.Combine(_testDirectory, "Test5.txt");
                File.WriteAllText(fileName5, "Test");
                string fileName6 = Path.Combine(_testDirectory, "Test6.txt");
                File.WriteAllText(fileName6, "Test");
                Thread.Sleep(50);
                Assert.AreEqual(1, events);
                Thread.Sleep(80);
                Assert.AreEqual(2, events);
                Assert.AreEqual(fileName5, args.NewFiles[0]);
                Assert.AreEqual(fileName6, args.NewFiles[1]);
                Assert.AreEqual(fileName3, args.ChangedFiles[0]);
                Assert.AreEqual(fileName4, args.ChangedFiles[1]);
                Assert.AreEqual(fileName1, args.DeletedFiles[0]);
                Assert.AreEqual(fileName2, args.DeletedFiles[1]);

                Thread.Sleep(200);
                Assert.AreEqual(2, events);
            };
        }


    }
}

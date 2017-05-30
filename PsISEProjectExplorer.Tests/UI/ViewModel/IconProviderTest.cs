using NUnit.Framework;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PsISEProjectExplorer.Tests.UI.ViewModel
{
    [TestFixture]
    public class IconProviderTest
    {

        private IconProvider iconProvider;

        private String testFileName;

        [OneTimeSetUp]
        public void Setup()
        {
            if (!UriParser.IsKnownScheme("pack"))
                new System.Windows.Application();
            this.iconProvider = new IconProvider();
            this.testFileName = Path.GetTempFileName();
            File.Create(testFileName).Close();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            File.Delete(testFileName);
        }

        [Test]
        public void GetImageSourceForFileSystemEntryShouldReturnShellIconWhenIsNotExcludedAndIsValid()
        {
            ImageSource result = iconProvider.GetImageSourceForFileSystemEntry(testFileName, false, true);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.AssignableFrom<DrawingImage>());
            Assert.That(result.Width, Is.GreaterThan(0));
        }

        [Test]
        public void GetImageSourceForFileSystemEntryShouldReturnOverlayedIconWhenIsNotExcludedAndIsNotValid()
        {
            ImageSource result = iconProvider.GetImageSourceForFileSystemEntry(testFileName, false, false);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableFrom<DrawingImage>());
            Assert.That(result.Width, Is.GreaterThan(0));
        }

        [Test]
        public void GetImageSourceForFileSystemEntryShouldReturnOverlayedIconWhenIsExcludedAndIsNotValid()
        {
            ImageSource result = iconProvider.GetImageSourceForFileSystemEntry(testFileName, true, false);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableFrom<DrawingImage>());
            Assert.That(result.Width, Is.GreaterThan(0));
        }

        [Test]
        public void GetImageSourceForFileSystemEntryShouldReturnOverlayedIconWhenIsExcludedAndIsValid()
        {
            ImageSource result = iconProvider.GetImageSourceForFileSystemEntry(testFileName, true, true);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableFrom<DrawingImage>());
            Assert.That(result.Width, Is.GreaterThan(0));
        }

        [Test]
        public void GetImageSourceForPowershellItemEntryShouldReturnResourceIconWhenNodeTypeIsValid()
        {
            var namesToTest = Enum.GetNames(typeof(NodeType)).Where(name => "Intermediate" != name);
            foreach (var name in namesToTest)
            {
                Console.WriteLine("IconProvider " + name);
                ImageSource result = iconProvider.GetImageSourceBasingOnNodeType(name);
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        public void GetImageSourceForPowershellItemEntryShouldReturnNullWhenNodeTypeIsNotValid()
        {
            ImageSource result = iconProvider.GetImageSourceBasingOnNodeType("x");
            Assert.That(result, Is.Null);
        }
    }
}

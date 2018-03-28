using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Common
{
    /// <summary>
    /// Test class for <see cref="FieldsCache"/>
    /// </summary>
    [TestClass]
    public class FieldsCacheTests
    {
        private dynamic _persistedFieldsCache;

        [TestInitialize]
        public void TestInitialize()
        {
            var instance = FieldsCache.Instance;

            _persistedFieldsCache = Utilities.Utility.Clone(ExposedClass.From(typeof(FieldsCache))._persistedFieldsCache as FieldsCache.Metadata);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ExposedClass.From(typeof(FieldsCache))._persistedFieldsCache = _persistedFieldsCache;
            ExposedClass.From(typeof(FieldsCache)).PersistFieldsMetadata();
        }

        #region GetVisibleFieldsForEntity

        [TestMethod]
        public void FieldsCacheTests_GetVisibleFieldsForEntity_NullEntityType_EmptyList()
        {
            ValidateFieldsInCache(null, new List<string>());
        }

        [TestMethod]
        public void FieldsCacheTests_GetVisibleFieldsForEntity_EmptyEntityType_EmptyList()
        {
            ValidateFieldsInCache(string.Empty, new List<string>());
        }

        [TestMethod]
        public void FieldsCacheTests_GetVisibleFieldsForEntity_DefaultEntityType_DefaultFields()
        {
            Dictionary<string, HashSet<string>> _defaultFieldsDictionary;
            _defaultFieldsDictionary = ExposedClass.From(typeof(FieldsCache))._defaultFieldsCache.data;
            ValidateFieldsInCache("task", _defaultFieldsDictionary["task"].ToList());
        }

        [TestMethod]
        public void FieldsCacheTests_GetVisibleFieldsForEntity_UnknownEntityType_EmptyList()
        {
            ValidateFieldsInCache("unknown", new List<string>());
        }

        #endregion

        #region AreSameFieldsAsDefaultFields

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_EmptyEntityType()
        {
            Assert.IsFalse(FieldsCache.Instance.AreSameFieldsAsDefaultFields("", null),
                "Empty entity type should be an invalid case");
        }

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_NullFieldsList()
        {
            Assert.IsFalse(FieldsCache.Instance.AreSameFieldsAsDefaultFields(Defect.SUBTYPE_DEFECT, null),
                "Null fields list should be an invalid case");
        }

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_UnknownEntityType()
        {
            Assert.IsFalse(FieldsCache.Instance.AreSameFieldsAsDefaultFields("unknown", new List<FieldViewModel>()),
                "Unknown entity type should be an invalid case");
        }

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_DifferentNumberOfFields()
        {
            Assert.IsFalse(FieldsCache.Instance.AreSameFieldsAsDefaultFields(Defect.SUBTYPE_DEFECT, new List<FieldViewModel>()),
                "Checking a different number of fields for a default entity type should be an invalid case");
        }

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_DifferentFields()
        {
            var entity = new WorkItem();
            Assert.IsFalse(FieldsCache.Instance.AreSameFieldsAsDefaultFields("requirement_document",
                new List<FieldViewModel>
                {
                    new FieldViewModel(entity, "release", "release", true),
                    new FieldViewModel(entity, "creation_time", "creation_time", true),
                    new FieldViewModel(entity, "last", "last", true)
                }),
                "Different field name should be an invalid case");
        }

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_NotAllFieldsAreSelected()
        {
            var entity = new WorkItem();
            Assert.IsFalse(FieldsCache.Instance.AreSameFieldsAsDefaultFields("requirement_document",
                new List<FieldViewModel>
                {
                    new FieldViewModel(entity, "release", "release", true),
                    new FieldViewModel(entity, "creation_time", "creation_time", false),
                    new FieldViewModel(entity, "last_modified", "last_modified", true)
                }),
                "Not all fields are selected should be an invalid case");
        }

        [TestMethod]
        public void FieldsCacheTests_AreSameFieldsAsDefaultFields_SameFields()
        {
            var entity = new WorkItem();
            Assert.IsTrue(FieldsCache.Instance.AreSameFieldsAsDefaultFields("requirement_document",
                new List<FieldViewModel>
                {
                    new FieldViewModel(entity, "release", "release", true),
                    new FieldViewModel(entity, "creation_time", "creation_time", true),
                    new FieldViewModel(entity, "last_modified", "last_modified", true)
                }),
                "Having all the correct fields should be a valid case");
        }

        #endregion

        #region ResetVisibleFieldsForEntity

        [TestMethod]
        public void FieldsCacheTests_ResetVisibleFieldsForEntity_UnknownEntityType()
        {
            FieldsCache.Instance.ResetVisibleFieldsForEntity("unknown");

            ValidateFieldsInCache("unknown", new List<string>());
        }

        [TestMethod]
        public void FieldsCacheTests_ResetVisibleFieldsForEntity_ExistingEntityType()
        {
            var entity = new WorkItem();
            FieldsCache.Instance.UpdateVisibleFieldsForEntity(Requirement.SUBTYPE_DOCUMENT,
                new List<FieldViewModel> { new FieldViewModel(entity, "name", "label", true) });

            FieldsCache.Instance.ResetVisibleFieldsForEntity(Requirement.SUBTYPE_DOCUMENT);
            ValidateFieldsInCache(Requirement.SUBTYPE_DOCUMENT, new List<string> { "release", "creation_time", "last_modified" });
        }

        [TestMethod]
        public void FieldsCacheTests_ResetVisibleFieldsForEntity_NonExistingDefaultEntityType()
        {
            var entity = new WorkItem();
            FieldsCache.Instance.UpdateVisibleFieldsForEntity("unknown",
                new List<FieldViewModel> { new FieldViewModel(entity, "name", "label", true) });

            FieldsCache.Instance.ResetVisibleFieldsForEntity("unknown");
            ValidateFieldsInCache("unknown", new List<string> { "name" });
        }

        #endregion

        #region UpdateVisibleFieldsForEntity

        [TestMethod]
        public void FieldsCacheTests_UpdateVisibleFieldsForEntity_NullEntityType()
        {
            FieldsCache.Instance.UpdateVisibleFieldsForEntity(null, null);
        }

        [TestMethod]
        public void FieldsCacheTests_UpdateVisibleFieldsForEntity_NullFields()
        {
            FieldsCache.Instance.UpdateVisibleFieldsForEntity("entity", null);
            ValidateFieldsInCache("entity", new List<string>());
        }

        [TestMethod]
        public void FieldsCacheTests_UpdateVisibleFieldsForEntity_NewEntityType()
        {
            var entity = new WorkItem();
            FieldsCache.Instance.UpdateVisibleFieldsForEntity("entity",
                new List<FieldViewModel> { new FieldViewModel(entity, "name", "label", true) });

            ValidateFieldsInCache("entity", new List<string> { "name" });
        }

        [TestMethod]
        public void FieldsCacheTests_UpdateVisibleFieldsForEntity_NotAllFieldsAreSelected()
        {
            var entity = new WorkItem();
            FieldsCache.Instance.UpdateVisibleFieldsForEntity(Requirement.SUBTYPE_DOCUMENT,
                new List<FieldViewModel>
                {
                    new FieldViewModel(entity, "release", "release", true),
                    new FieldViewModel(entity, "creation_time", "creation_time", false),
                    new FieldViewModel(entity, "last_modified", "last_modified", true)
                });

            ValidateFieldsInCache(Requirement.SUBTYPE_DOCUMENT, new List<string> { "release", "last_modified" });
        }

        #endregion

        private void ValidateFieldsInCache(string entityType, List<string> expectedFields)
        {
            var fields = FieldsCache.Instance.GetVisibleFieldsForEntity(entityType).ToList();
            CollectionAssert.AreEqual(expectedFields, fields, $"Mismatched elements in cache for entity {entityType}");
        }
    }
}

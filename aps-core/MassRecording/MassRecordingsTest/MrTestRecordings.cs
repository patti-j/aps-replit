using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MassRecordingsTest;

[TestClass]
public partial class MrTest
{
    /// <summary>
    /// Pick a customer and a recording to run and launch the recording helper
    /// </summary>
    [DataRow(@"V600\Customers\ABM Canada, Inc.ABM Canada, Inc", "2011.01.28.15.11.30.scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ABMCanadaIncABMCanadaInc_20110128151130scenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\ACS", "0000000000._2010.11.22.14.50.18.scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ACS_0000000000_20101122145018scenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\ACS", "2010.11.18.15.14.17")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ACS_20101118151417(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\ACS", "2010.11.18.15.20.27")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ACS_20101118152027(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\ACS", "Production")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ACS_Production(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "2018.02.23T17.05.37.789 Part 1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_20180223T170537789Part1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "2018.02.23T17.05.37.789 Part 2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_20180223T170537789Part2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "2018.02.23T17.05.37.789 Part 3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_20180223T170537789Part3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "2018.02.23T17.05.37.789 Part 4")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_20180223T170537789Part4(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "AFL-ScheduleOutOfSequence-2017.08.23T14.41.16.699")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_AFLScheduleOutOfSequence20170823T144116699(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "Lots")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_Lots(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "PART1_0000000009.PT.ERPTransmissions.PerformImportCompletedT")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_PART1_0000000009PTERPTransmissionsPerformImportCompletedT(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "PART2_0000000009.PT.ERPTransmissions.PerformImportCompletedT")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_PART2_0000000009PTERPTransmissionsPerformImportCompletedT(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFL", "PART3_0000000009.PT.ERPTransmissions.PerformImportCompletedT")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFL_PART3_0000000009PTERPTransmissionsPerformImportCompletedT(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\AFS", "2016.01.11T16.28.02.132-18th morning only-2015.01")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AFS_20160111T16280213218thmorningonly201501(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\American Press", "2011.02.02T11.42.58.765")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AmericanPress_20110202T114258765(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\American Press", "2011.02.02T12.41.28.046")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AmericanPress_20110202T124128046(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\American Press", "2011.02.02T13.15.34.375")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AmericanPress_20110202T131534375(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\American Press", "2011.02.02T14.20.41.703")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AmericanPress_20110202T142041703(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\American Press", "2011.02.02T16.51.48.875")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_AmericanPress_20110202T165148875(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\BG Products", "0000000000._2016.06.10.14.03.52.scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_BGProducts_0000000000_20160610140352scenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\CII - Cereal Ingredients", "2015.10.01T15.15.45.884")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_CIICerealIngredients_20151001T151545884(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\EFS - Evans Food Group", "2011.05.20T05.57.52.260")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_EFSEvansFoodGroup_20110520T055752260(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\EFS - Evans Food Group", "2011.05.24T09.11.50.599")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_EFSEvansFoodGroup_20110524T091150599(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\FNB", "2013.04.03T17.36.14.213FNOptimizeResultsInOverlappingTankOperations")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_FNB_20130403T173614213FNOptimizeResultsInOverlappingTankOperations(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\FNB", "2013.04.04T08.25.28.856F&NOptimizeTankNegOverlapTransSpan")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_FNB_20130404T082528856FNOptimizeTankNegOverlapTransSpan(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\FNB", "MixAndPackCustomization")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_FNB_MixAndPackCustomization(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.10.08T19.41.05.295")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151008T194105295(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.10.20T06.36.45.846 Part 1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151020T063645846Part1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.10.20T06.36.45.846 Part 2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151020T063645846Part2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.11.13T13.55.08.353")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151113T135508353(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.11.13T13.59.41.151")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151113T135941151(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.11.17T06.32.36.002")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151117T063236002(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.11.17T19.12.49.706")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151117T191249706(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.12.07T11.06.44.057 Part 1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151207T110644057Part1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.12.07T11.06.44.057 Part 2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151207T110644057Part2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.12.07T11.06.44.057 Part 3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151207T110644057Part3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.12.07T11.06.44.057 Part 4")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151207T110644057Part4(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPC - Graham Packaging Company", "2015.12.07T11.06.44.057 Part 5")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPCGrahamPackagingCompany_20151207T110644057Part5(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part4")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart4(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part5")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart5(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part6")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart6(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part7")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart7(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part8")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart8(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "FTS Part9")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_FTSPart9(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "GPI-CST-2016.03.29T08.32.57.810-crewing helper comes and goes")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_GPICST20160329T083257810crewinghelpercomesandgoes(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "KVL_PART 1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_KVL_PART1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "KVL_PART 2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_KVL_PART2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "KVL_PART 3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_KVL_PART3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "KVL_PART 4")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_KVL_PART4(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "KVL_PART 5")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_KVL_PART5(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "MIT_PART 1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_MIT_PART1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "MIT_PART 2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_MIT_PART2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "MIT_PART 3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_MIT_PART3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "MIT_PART 4")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_MIT_PART4(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "MIT_PART 5")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_MIT_PART5(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "REN Part1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_RENPart1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "REN Part2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_RENPart2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "SOL Part1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_SOLPart1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "SOL Part2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_SOLPart2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "SOL Part3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_SOLPart3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GPI", "SOL Part4")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GPI_SOLPart4(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "0000000021._2012.02.24.22.05.20.scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_0000000021_20120224220520scenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "GUA_022412")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_GUA_022412(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "MRP 2012.5.9")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_MRP201259(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "OptimizedBugSecond")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_OptimizedBugSecond(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "QAResult3")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_QAResult3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "Qty Infinity")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_QtyInfinity(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\GUA", "ResUserTimeSpan")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_GUA_ResUserTimeSpan(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\HarmonicDrive", "multimove optimize")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_HarmonicDrive_multimoveoptimize(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\KAO", "Kao scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KAO_Kaoscenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.06.29T08.50.55.804")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110629T085055804(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.06.29T08.58.06.525")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110629T085806525(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.06.29T13.56.14.686")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110629T135614686(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.06.29T14.36.51.498")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110629T143651498(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.06.29T14.38.21.448")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110629T143821448(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.07.01T06.45.21.322")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110701T064521322(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.07.27T13.04.14.639")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110727T130414639(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.07.27T13.10.26.391")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110727T131026391(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\King's Hawaiian.King's Hawaiian Test Server", "2011.07.28T06.48.48.793")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_KingsHawaiianKingsHawaiianTestServer_20110728T064848793(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Libman", "5794")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Libman_5794(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Libman", "Libman 2016.3.8")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Libman_Libman201638(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Libman", "Libman Mass Recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Libman_LibmanMassRecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Libman", "Libman Recordings 2015.10.19")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Libman_LibmanRecordings20151019(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_05P-Recordings-2018.07.20")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_05PRecordings20180720(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P-Recordings-2018.07.20")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06PRecordings20180720(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.06.08T12.51.40.000 (1)")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180608T1251400001(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.06.08T12.51.40.000")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180608T125140000(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.06.19T10.08.36.575")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180619T100836575(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.10T20.14.31.394")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180710T201431394(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.10T21.08.04.525")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180710T210804525(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.12T11.50.22.679")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180712T115022679(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.15T17.12.24.127")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180715T171224127(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.17T10.43.24.958")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180717T104324958(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.18T08.17.17.488")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180718T081717488(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.18T14.08.18.261")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180718T140818261(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_06P_MRP_Test-2018.07.20-2018.07.20T19.53.07.549")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06P_MRP_Test2018072020180720T195307549(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_07P-2017.08.21-2018.06.12T12.50.23.964")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07P2017082120180612T125023964(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_07P-2017.08.21-2018.07.03T12.46.33.483")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07P2017082120180703T124633483(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_07P-2017.08.21-2018.07.15T17.13.15.603")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07P2017082120180715T171315603(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_13P-2018.07.22-Recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13P20180722Recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_14P-2018.07.22-Recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14P20180722Recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_14P_MRP_Test-2018.07.22-2018.06.08T13.01.27.462")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14P_MRP_Test2018072220180608T130127462(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_14P_MRP_Test-2018.07.22-2018.06.13T14.15.46.753")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14P_MRP_Test2018072220180613T141546753(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_15P-2018.07.22-Recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15P20180722Recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_15P_MRP_Test-2018.07.22-2018.06.08T13.01.57.811")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15P_MRP_Test2018072220180608T130157811(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_15P_MRP_Test-2018.07.22-2018.06.13T14.17.10.711")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15P_MRP_Test2018072220180613T141710711(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston", "CALW_16P-2018.07.22-Recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_16P20180722Recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_05P-Recordings-2018.04.05", "2018.03.08T11.18.03.223")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_05PRecordings20180405_20180308T111803223(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_05P-Recordings-2018.04.05", "2018.03.12T19.43.48.875")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_05PRecordings20180405_20180312T194348875(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_05P-Recordings-2018.04.05", "2018.03.25T22.06.05.810")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_05PRecordings20180405_20180325T220605810(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_05P-Recordings-2018.04.05", "2018.03.26T09.27.01.197")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_05PRecordings20180405_20180326T092701197(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_05P-Recordings-2018.04.05", "2018.04.02T08.14.00.737")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_05PRecordings20180405_20180402T081400737(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_06P-Recordings-2018.04.05", "2018.03.08T11.19.26.657")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06PRecordings20180405_20180308T111926657(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_06P-Recordings-2018.04.05", "2018.03.12T19.45.12.783")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06PRecordings20180405_20180312T194512783(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_06P-Recordings-2018.04.05", "2018.03.25T22.06.41.206")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06PRecordings20180405_20180325T220641206(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_06P-Recordings-2018.04.05", "2018.03.26T10.36.52.354")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06PRecordings20180405_20180326T103652354(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_06P-Recordings-2018.04.05", "2018.04.02T08.14.00.909")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_06PRecordings20180405_20180402T081400909(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_07P-Recordings-2018.04.05", "2018.03.08T11.24.33.377")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07PRecordings20180405_20180308T112433377(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_07P-Recordings-2018.04.05", "2018.03.12T19.46.06.924")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07PRecordings20180405_20180312T194606924(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_07P-Recordings-2018.04.05", "2018.03.25T22.07.21.441")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07PRecordings20180405_20180325T220721441(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_07P-Recordings-2018.04.05", "2018.03.26T10.41.56.359")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07PRecordings20180405_20180326T104156359(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_07P-Recordings-2018.04.05", "2018.04.02T08.14.00.940")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_07PRecordings20180405_20180402T081400940(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_13P-Recordings-2018.04.05", "2018.03.08T10.47.30.139")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13PRecordings20180405_20180308T104730139(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_13P-Recordings-2018.04.05", "2018.03.12T19.46.51.123")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13PRecordings20180405_20180312T194651123(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_13P-Recordings-2018.04.05", "2018.03.25T22.11.17.679")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13PRecordings20180405_20180325T221117679(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_13P-Recordings-2018.04.05", "2018.03.26T10.44.53.582")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13PRecordings20180405_20180326T104453582(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_13P-Recordings-2018.04.05", "2018.04.02T08.14.00.909")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13PRecordings20180405_20180402T081400909(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_13P-Recordings-2018.04.05", "CALW_06P_MRP_Test-2018.07.20-2018.07.18T08.17.17.488")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_13PRecordings20180405_CALW_06P_MRP_Test2018072020180718T081717488(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_14P-Recordings-2018.04.05", "2018.03.08T10.48.57.749")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14PRecordings20180405_20180308T104857749(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_14P-Recordings-2018.04.05", "2018.03.12T19.47.31.474")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14PRecordings20180405_20180312T194731474(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_14P-Recordings-2018.04.05", "2018.03.25T22.13.05.973")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14PRecordings20180405_20180325T221305973(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_14P-Recordings-2018.04.05", "2018.03.26T10.50.20.414")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14PRecordings20180405_20180326T105020414(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_14P-Recordings-2018.04.05", "2018.04.02T08.14.00.878")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_14PRecordings20180405_20180402T081400878(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_15P-Recordings-2018.04.05", "2018.03.12T18.24.07.408")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15PRecordings20180405_20180312T182407408(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_15P-Recordings-2018.04.05", "2018.03.12T19.48.20.528")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15PRecordings20180405_20180312T194820528(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_15P-Recordings-2018.04.05", "2018.03.25T22.13.09.412")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15PRecordings20180405_20180325T221309412(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_15P-Recordings-2018.04.05", "2018.04.02T08.14.00.847")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15PRecordings20180405_20180402T081400847(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_15P-Recordings-2018.04.05", "CALW_06P_MRP_Test-2018.07.20-2018.07.10T21.08.04.525")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15PRecordings20180405_CALW_06P_MRP_Test2018072020180710T210804525(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_15P-Recordings-2018.04.05", "CALW_06P_MRP_Test-2018.07.20-2018.07.15T17.12.24.127")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_15PRecordings20180405_CALW_06P_MRP_Test2018072020180715T171224127(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_16P-Recordings-2018.04.05", "2018.03.12T19.49.31.672")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_16PRecordings20180405_20180312T194931672(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_16P-Recordings-2018.04.05", "2018.03.24T10.20.02.535")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_16PRecordings20180405_20180324T102002535(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_16P-Recordings-2018.04.05", "2018.03.25T22.13.12.687")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_16PRecordings20180405_20180325T221312687(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_16P-Recordings-2018.04.05", "2018.03.26T10.56.09.544")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_16PRecordings20180405_20180326T105609544(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\LW - Lamb Weston\CALW_16P-Recordings-2018.04.05", "2018.04.02T08.14.00.753")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_LWLambWeston_CALW_16PRecordings20180405_20180402T081400753(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\MC - Malley's Chocolates", "MAL-2017.08.01T12.21.25.134")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_MCMalleysChocolates_MAL20170801T122125134partialdropconnectionerror(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Morrison", "Infinite Loop")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Morrison_InfiniteLoop(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\MOTT", "Mott - 2013.02.05 - recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_MOTT_Mott20130205recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NATT", "2015.04.30T09.26.30.851-NATT-2015.04.30")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NATT_20150430T092630851NATT20150430(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NATT", "NATTSimpleSampleForCustomizationScenario-tfs1129")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NATT_NATTSimpleSampleForCustomizationScenariotfs1129(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-01")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090801(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-02")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090802(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-03")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090803(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-04")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090804(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-05")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090805(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-06")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090806(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-07")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090807(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-08")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090808(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-09")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090809(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-10")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090810(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-11")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090811(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-12")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090812(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-13")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090813(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-14")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090814(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-15")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090815(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-16")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090816(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-17")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090817(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-18")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090818(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.13T10.39.22.085-NBB-WETHOP-v2014.11.6.1-2015.09.08-19")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150813T103922085NBBWETHOPv201411612015090819(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-01")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090801(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-02")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090802(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-03")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090803(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-04")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090804(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-05")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090805(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-06")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090806(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-07")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090807(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-08")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090808(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-09")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090809(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\NBB", "2015.08.28T14.38.58.344-NBB-SNOWMASS-v2014.11.6.1-2015.09.08-10")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_NBB_20150828T143858344NBBSNOWMASSv201411612015090810(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\PolyCello", "11.19")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_PolyCello_1119(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\PolyCello", "11.20")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_PolyCello_1120(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\PolyCello", "FullDayRecordings 2014.1.21")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_PolyCello_FullDayRecordings2014121(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\PolyCello", "poly (216480 215452 214433)")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_PolyCello_poly216480215452214433(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\PolyCello", "Polycello Scenario for Larry")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_PolyCello_PolycelloScenarioforLarry(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Preferred Meal Systems Inc", "Moosic Server")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_PreferredMealSystemsInc_MoosicServer(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\QED", "QED - 2017.05.02T16.01.15.608 - 0511 morning")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_QED_QED20170502T1601156080511morning(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Ross Environmental", "2016.10.27T08.35.17.089")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_RossEnvironmental_20161027T083517089(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Saint Gobain", "Optimize")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_SaintGobain_Optimize(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\SCPC - Sugar Creek Packing Company", "SCPC recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_SCPCSugarCreekPackingCompany_SCPCrecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Synthes", "Synthes")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Synthes_Synthes(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.04.07-Optimize")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150407Optimize(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.05.13-Veridiam DeSync UserError-2015.05.13")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150513VeridiamDeSyncUserError20150513(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.08.07-45421-22 falied to schedule")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_201508074542122faliedtoschedule(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.08.23T05.32.20.770-01-Veridiam")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150823T05322077001Veridiam(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.08.23T05.32.20.770-02-Veridiam")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150823T05322077002Veridiam(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.08.23T05.32.20.770-03-Veridiam")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150823T05322077003Veridiam(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.09.07-2015.9.8 Recordings")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150907201598Recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.09.08 import and move")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20150908importandmove(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.11.19T08.32.02.995")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20151119T083202995(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2015.11.19T09.07.53.701")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20151119T090753701(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.01.15-Veridiam 2016.1.15")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160115Veridiam2016115(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.01.30-VER-2016.01.29T07.29.34.329")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160130VER20160129T072934329(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.02.20T09.34.40.250")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160220T093440250(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.02.24T18.50.21.119")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160224T185021119(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.02.25-Veridiam Recordings - 2016.02.22")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160225VeridiamRecordings20160222(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.03.13T10.14.20.070 - order 48421")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160313T101420070order48421(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.03.13T10.14.20.070")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160313T101420070(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.03.30T11.38.59.627")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160330T113859627(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "2016.06.09T14.31.18.152")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_20160609T143118152(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "Optimize")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_Optimize(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "VER-2016.01.29T07.29.34.329")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_VER20160129T072934329(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "Veridiam 2016.1.15")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_Veridiam2016115(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Veridiam", "weekAug2015")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_Veridiam_weekAug2015(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Zenith Specialty Bag.Zenith Specialty Bag", "0000000008._2010.11.12.16.14.40.scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ZenithSpecialtyBagZenithSpecialtyBag_0000000008_20101112161440scenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\ZII - Zotos International Inc", "ZOTOS_APS_PROD-2016.3.10.1-2018.01.26T12.25.19.521_PART 1")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ZIIZotosInternationalInc_ZOTOS_APS_PROD2016310120180126T122519521_PART1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\ZII - Zotos International Inc", "ZOTOS_APS_PROD-2016.3.10.1-2018.01.26T12.25.19.521_PART 2")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ZIIZotosInternationalInc_ZOTOS_APS_PROD2016310120180126T122519521_PART2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\Customers\Zim's Bagging Company.Zim's Bagging Company", "0000000319._2010.09.03.17.27.53.scenarios")]
    [DataTestMethod]
    [TestCategory("Customers")]
    public void V600_Customers_ZimsBaggingCompanyZimsBaggingCompany_0000000319_20100903172753scenarios(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\Inventory LeadTime", "handmade recordings")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_InventoryLeadTime_handmaderecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\Item LeadTime", "handmade recordings")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_ItemLeadTime_handmaderecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\MaterialConstraint", "Handmade recordings")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_MaterialConstraint_Handmaderecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\MaxDelay", "2015.11.05T08.16.36.950SimpleMaxDelayJob4DelayedByJob3")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_MaxDelay_20151105T081636950SimpleMaxDelayJob4DelayedByJob3(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\OptimizePlant", "2018.01.21T20.51.28.795OptimizeLunaPlant")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_OptimizePlant_20180121T205128795OptimizeLunaPlant(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\ResourceVsOperationSelection", "ResourceVsOperationSelection")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_ResourceVsOperationSelection_ResourceVsOperationSelection(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\FeatureTesting\Tank", "ConsumingOperationInSameMO")]
    [DataTestMethod]
    [TestCategory("FeatureTesting")]
    public void V600_FeatureTesting_Tank_ConsumingOperationInSameMO(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Alex B", "2015.11.02T09.09.29.251")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_AlexB_20151102T090929251(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Alex B", "2015.11.20T11.21.34.195")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_AlexB_20151120T112134195(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Alex B", "2016.02.25T15.44.53.763")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_AlexB_20160225T154453763(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Alex B", "2016.03.02T10.56.17.767")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_AlexB_20160302T105617767(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Amin", "2015.12.11T11.41.38.073")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Amin_20151211T114138073(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Cavan", "2015.12.11T10.52.51.193")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Cavan_20151211T105251193(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Cavan", "2016.02.03T14.50.18.117_PART 1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Cavan_20160203T145018117_PART1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Cavan", "2016.02.03T14.50.18.117_PART 2")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Cavan_20160203T145018117_PART2(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\JD", "2017.02.16T08.45.04.841")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_JD_20170216T084504841(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Jim", "2015.11.25T18.29.01.838")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Jim_20151125T182901838(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "2015.07.09T09.57.04.675")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_20150709T095704675(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "2016.06.23T15.56.14.251-CAF-ForecastExample")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_20160623T155614251CAFForecastExample(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-Analytics 2015.11.1.1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsAnalytics20151111(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-AX2012 2015.11.1.1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsAX201220151111(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-AXv500 2013.10.25.1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsAXv500201310251(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-Graham Packaging 2015.9.21.1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsGrahamPackaging20159211(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-QC_1105 11.0.5")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsQC_11051105(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-SCPC_0413 2015.4.13.1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsSCPC_041320154131(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\John", "Recordings-Veridiam_CTP_Labor 2015.11.1.1")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_John_RecordingsVeridiam_CTP_Labor20151111(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Ryan", "2015.11.07T16.55.40.379")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Ryan_20151107T165540379(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\PlanetTogether\Ryan", "DeSync")]
    [DataTestMethod]
    [TestCategory("PlanetTogether")]
    public void V600_PlanetTogether_Ryan_DeSync(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "00061MultWHAlloc")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_00061MultWHAlloc(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "00063")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_00063(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "01184OnePlantPushesJobsOutInThe2dPlant")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_01184OnePlantPushesJobsOutInThe2dPlant(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "01874TankSucOvlp")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_01874TankSucOvlp(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02286")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02286(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02648")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02648(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02693MOShippingBufferOverride")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02693MOShippingBufferOverride(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02727")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02727(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02737DeptFrozenSpan")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02737DeptFrozenSpan(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02819")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02819(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "02870")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_02870(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03009")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03009(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03010")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03010(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03039")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03039(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03157StartedAltPathNotSelected")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03157StartedAltPathNotSelected(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03238")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03238(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03303")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03303(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03377")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03377(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03406")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03406(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03407")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03407(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03439")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03439(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03467")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03467(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03477")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03477(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03504")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03504(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03519")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03519(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03539")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03539(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03604")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03604(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03738")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03738(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03746")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03746(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03751")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03751(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03752")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03752(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03761")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03761(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03764")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03764(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03764JFEWithTransmissionsAndOptimize")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03764JFEWithTransmissionsAndOptimize(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03837")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03837(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03878")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03878(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03909")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03909(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "03994")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_03994(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "04062")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_04062(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "04077")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_04077(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "04110")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_04110(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "04216")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_04216(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "04255")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_04255(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "04369")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_04369(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "07526")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_07526(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "09025_AdjustPOReceiptDateNotAffectingJobsStartDate")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_09025_AdjustPOReceiptDateNotAffectingJobsStartDate(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "09025_POsNotAddedToInv_JobsGetMatlAtPH")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_09025_POsNotAddedToInv_JobsGetMatlAtPH(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "09225")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_09225(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "09254")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_09254(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "1986")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_1986(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "2249")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_2249(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "2990MoveMultBlocks")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_2990MoveMultBlocks(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3429")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3429(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3435")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3435(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3599")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3599(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3639")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3639(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3762")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3762(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3892")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3892(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "3961")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_3961(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4047")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4047(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4154")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4154(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4280")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4280(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4415")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4415(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4437")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4437(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4440")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4440(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4453")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4453(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4455")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4455(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4501")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4501(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4543")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4543(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4961")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4961(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "4973")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_4973(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5018")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5018(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5040")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5040(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5064")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5064(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5088")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5088(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5429")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5429(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5620_CostRuleWeights")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5620_CostRuleWeights(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5750")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5750(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5770")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5770(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "5794")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_5794(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "6002")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_6002(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "6065")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_6065(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "6619-NullBatchException")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_6619NullBatchException(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "7886")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_7886(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "7903 Campaign Move")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_7903CampaignMove(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "9150")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9150(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "9151")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9151(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "9160")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9160(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "9236")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9236(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks", "AAPublishTest")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_AAPublishTest(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\8732", "8732 QA 1")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_8732_8732QA1(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9135", "Stein Seal 5-18-18")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9135_SteinSeal51818(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9157", "sales order optimize")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9157_salesorderoptimize(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9220", "pruned recording")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9220_prunedrecording(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9220", "recordings")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9220_recordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9221", "handmade recording")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9221_handmaderecording(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9250", "hand made recordings")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9250_handmaderecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9251", "pruned recordings")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9251_prunedrecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9252", "handmade recording")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9252_handmaderecording(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }

    [DataRow(@"V600\TFSTasks\9281", "pruned recordings")]
    [DataTestMethod]
    [TestCategory("TFSTasks")]
    public void V600_TFSTasks_9281_prunedrecordings(string a_path, string a_zip)
    {
        TestProcessor processor = new (s_instanceConfig);
        processor.SetFullRecordingPath(a_path, a_zip);
        processor.Test();
        processor.CheckForErrors();
    }
}
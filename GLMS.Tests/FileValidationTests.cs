namespace GLMS.Tests;

public class FileValidationTests
{
    private static bool IsPdf(string fileName) =>
        Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    [Fact]
    public void PdfLowercase_IsValid()
    {
        Assert.True(IsPdf("signed_agreement.pdf"));
    }

    [Fact]
    public void PdfUppercase_IsValid()
    {
        Assert.True(IsPdf("signed_agreement.PDF"));
    }

    [Fact]
    public void ExeFile_IsInvalid()
    {
        Assert.False(IsPdf("malicious.exe"));
    }

    [Fact]
    public void DocxFile_IsInvalid()
    {
        Assert.False(IsPdf("contract_draft.docx"));
    }

    [Theory]
    [InlineData("agreement.pdf",       true)]
    [InlineData("agreement.PDF",       true)]
    [InlineData("Agreement.Pdf",       true)]
    [InlineData("malicious.exe",       false)]
    [InlineData("contract_draft.docx", false)]
    [InlineData("spreadsheet.xlsx",    false)]
    [InlineData("photo.png",           false)]
    [InlineData("archive.zip",         false)]
    public void FileExtensionValidation_CorrectlyIdentifiesPdf(string fileName, bool expectedValid)
    {
        Assert.Equal(expectedValid, IsPdf(fileName));
    }
   
}

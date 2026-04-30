using System.IO;
using Microsoft.Net.Http.Headers;
using web.Helpers;
using Xunit;

namespace core.UnitTests;

public class MultipartRequestHelperTests
{
    [Fact]
    public void GetBoundary_ValidBoundary_ReturnsBoundaryString()
    {
        var contentType = new MediaTypeHeaderValue("multipart/form-data");
        contentType.Boundary = "----WebKitFormBoundary";

        var boundary = MultipartRequestHelper.GetBoundary(contentType);

        Assert.Equal("----WebKitFormBoundary", boundary);
    }

    [Fact]
    public void GetBoundary_NullBoundary_ThrowsInvalidDataException()
    {
        var contentType = new MediaTypeHeaderValue("multipart/form-data");
        contentType.Boundary = null;

        Assert.Throws<InvalidDataException>(() => MultipartRequestHelper.GetBoundary(contentType));
    }

    [Fact]
    public void GetBoundary_EmptyBoundary_ThrowsInvalidDataException()
    {
        var contentType = new MediaTypeHeaderValue("multipart/form-data");
        contentType.Boundary = string.Empty;

        Assert.Throws<InvalidDataException>(() => MultipartRequestHelper.GetBoundary(contentType));
    }

    [Fact]
    public void IsMultipartContentType_MultipartFormData_ReturnsTrue()
    {
        var contentType = "multipart/form-data; boundary=----WebKitFormBoundary";

        var result = MultipartRequestHelper.IsMultipartContentType(contentType);

        Assert.True(result);
    }

    [Fact]
    public void IsMultipartContentType_MultipartMixed_ReturnsTrue()
    {
        var contentType = "multipart/mixed";

        var result = MultipartRequestHelper.IsMultipartContentType(contentType);

        Assert.True(result);
    }

    [Fact]
    public void IsMultipartContentType_ApplicationJson_ReturnsFalse()
    {
        var contentType = "application/json";

        var result = MultipartRequestHelper.IsMultipartContentType(contentType);

        Assert.False(result);
    }

    [Fact]
    public void IsMultipartContentType_NullContentType_ReturnsFalse()
    {
        var result = MultipartRequestHelper.IsMultipartContentType(null);

        Assert.False(result);
    }

    [Fact]
    public void IsMultipartContentType_EmptyContentType_ReturnsFalse()
    {
        var result = MultipartRequestHelper.IsMultipartContentType(string.Empty);

        Assert.False(result);
    }

    [Fact]
    public void HasFormDataContentDisposition_ValidFormData_ReturnsTrue()
    {
        var disposition = ContentDispositionHeaderValue.Parse("form-data; name=\"field\"");

        var result = MultipartRequestHelper.HasFormDataContentDisposition(disposition);

        Assert.True(result);
    }

    [Fact]
    public void HasFormDataContentDisposition_WithFileName_ReturnsFalse()
    {
        var disposition = ContentDispositionHeaderValue.Parse("form-data; name=\"file\"; filename=\"test.txt\"");

        var result = MultipartRequestHelper.HasFormDataContentDisposition(disposition);

        Assert.False(result);
    }

    [Fact]
    public void HasFormDataContentDisposition_InvalidDisposition_ReturnsFalse()
    {
        var disposition = ContentDispositionHeaderValue.Parse("attachment; filename=\"test.txt\"");

        var result = MultipartRequestHelper.HasFormDataContentDisposition(disposition);

        Assert.False(result);
    }

    [Fact]
    public void HasFormDataContentDisposition_NullDisposition_ReturnsFalse()
    {
        ContentDispositionHeaderValue? disposition = null;

        var result = MultipartRequestHelper.HasFormDataContentDisposition(disposition);

        Assert.False(result);
    }

    [Fact]
    public void HasFileContentDisposition_ValidFileUpload_ReturnsTrue()
    {
        var disposition = ContentDispositionHeaderValue.Parse("form-data; name=\"file\"; filename=\"document.pdf\"");

        var result = MultipartRequestHelper.HasFileContentDisposition(disposition);

        Assert.True(result);
    }

    [Fact]
    public void HasFileContentDisposition_ValidFileUploadWithStar_ReturnsTrue()
    {
        var disposition = new ContentDispositionHeaderValue("form-data");
        disposition.Name = "\"file\"";
        disposition.FileName = "\"test.txt\"";

        var result = MultipartRequestHelper.HasFileContentDisposition(disposition);

        Assert.True(result);
    }

    [Fact]
    public void HasFileContentDisposition_FormDataWithoutFile_ReturnsFalse()
    {
        var disposition = ContentDispositionHeaderValue.Parse("form-data; name=\"field\"");

        var result = MultipartRequestHelper.HasFileContentDisposition(disposition);

        Assert.False(result);
    }

    [Fact]
    public void HasFileContentDisposition_AttachmentDisposition_ReturnsFalse()
    {
        var disposition = ContentDispositionHeaderValue.Parse("attachment; filename=\"file.zip\"");

        var result = MultipartRequestHelper.HasFileContentDisposition(disposition);

        Assert.False(result);
    }

    [Fact]
    public void HasFileContentDisposition_NullDisposition_ReturnsFalse()
    {
        ContentDispositionHeaderValue? disposition = null;

        var result = MultipartRequestHelper.HasFileContentDisposition(disposition);

        Assert.False(result);
    }
}

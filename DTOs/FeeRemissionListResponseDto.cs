namespace NCMISAPI.DTOs;

public class FeeRemissionListResponseDto
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "OK";

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public bool IsSurveyor { get; set; }
    public bool IsSeen { get; set; }
    public string? RequestCreatorURL { get; set; }
    public bool IsRequestCreator { get; set; }

    public List<FeeRemissionListItemDto> Items { get; set; } = [];
}

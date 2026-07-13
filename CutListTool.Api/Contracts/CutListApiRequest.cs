using CutListTool.Core.Models.OutsideDataHandlers;

namespace CutListTool.Api.Contracts;

public sealed record CutListApiRequest(
    CutListInputData Input,
    CutListRequest Request
);
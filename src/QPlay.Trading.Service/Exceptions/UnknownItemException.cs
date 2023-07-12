using System;

namespace QPlay.Trading.Service.Exceptions;

[Serializable]
class UnknownItemException : Exception
{
    public UnknownItemException(Guid itemId)
        : base($"Unknown item '{itemId}'")
    {
        ItemId = itemId;
    }

    public Guid ItemId { get; }
}
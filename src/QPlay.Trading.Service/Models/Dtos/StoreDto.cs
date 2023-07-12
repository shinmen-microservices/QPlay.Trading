using System.Collections.Generic;

namespace QPlay.Trading.Service.Models.Dtos;

public record StoreDto(IEnumerable<StoreItemDto> Items, decimal UserGil);
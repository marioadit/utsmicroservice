using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Universal
{
    public interface IEntity
    {
        Guid Id { get; init; }

    }
}
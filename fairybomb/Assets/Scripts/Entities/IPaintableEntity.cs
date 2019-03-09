using System;
using System.Collections.Generic;


public interface IPaintableEntity
{
    PaintableTrait PaintableTrait { get; }

    void AppliedPaint(PaintData data);
    void RemovedPaint(PaintData data);
    float UpdatedPaint(PaintData data, float ticks);
}

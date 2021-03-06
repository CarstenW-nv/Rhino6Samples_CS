﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("e91781cd-e748-4a77-80fd-1658d2c5a89e")]
  public class SampleCsCurveDiscontinuity : Command
  {
    public override string EnglishName
    {
      get { return "SampleCsCurveDiscontinuity"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var go = new GetObject();
      go.SetCommandPrompt("Select curves");
      go.GeometryFilter = ObjectType.Curve;
      go.Get();
      if (go.CommandResult() != Result.Success)
        return go.CommandResult();

      var curve = go.Object(0).Curve();
      if (null == curve)
        return Result.Failure;

      var continuity = Continuity.G1_continuous;

      var gd = new GetOption();
      gd.SetCommandPrompt("Discontinuity to search");
      gd.AddOptionEnumList("Discontinuity", continuity);
      gd.AcceptNothing(true);
      var res = gd.Get();
      if (res == GetResult.Option)
      {
        var option = gd.Option();
        if (null == option)
          return Result.Failure;

        var list = Enum.GetValues(typeof (Continuity)).Cast<Continuity>().ToList();
        continuity = list[option.CurrentListOptionIndex];
      }
      else if (res != GetResult.Nothing)
        return Result.Cancel;

      var t0 = curve.Domain.Min;
      var t1 = curve.Domain.Max;
      for (; ; )
      {
        double t;
        var rc = curve.GetNextDiscontinuity(continuity, t0, t1, out t);
        if (rc)
        {
          doc.Objects.AddPoint(curve.PointAt(t));
          t0 = t;
        }
        else
          break;
      }

      doc.Views.Redraw();

      return Result.Success;
    }
  }
}

using Irony.Parsing;
using MW.Audio;
using MW.Helpers;
using MW.Parsing;
using MW.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Containers
    {
        [Function(name: "t", astType: AstType.Container, description: "Create track")]
        public static Container CreateTrack(MethodContext context)
        {
            var container = new Container();
            AddChildren(context, container, 0);

            return container;
        }

        [Function(name: "a", astType: AstType.Container, description: "Add element")]
        public static Container AddElement(MethodContext context)
        {
            if (context.Args.Count < 2)
            {
                throw new RunException($"{nameof(AddElement)} requires 2 or more arguments");
            }

            var container = context.Args[0].Evaluate<Container>(context.Thread);
            if (container == null)
            {
                throw new RunException($"{nameof(AddElement)} first argument must be a {typeof(Song)} or {typeof(Container)}");
            }

            AddChildren(context, container, 1);

            return container;
        }

        [Function(name: "getoffset", astType: AstType.Number, description: "Get the offset of an element in a song/track")]
        public static double GetOffset(MethodContext context)
        {
            if (context.Args.Count != 2)
            {
                throw new RunException($"{nameof(GetOffset)} requires 2 arguments");
            }

            var container = context.Args[0].Evaluate<Container>(context.Thread);
            if (container == null)
            {
                throw new RunException($"{nameof(GetOffset)} first argument must be a song or track");
            }

            if (container.Instances.Count == 0)
            {
                throw new RunException($"{nameof(GetOffset)} song/track has no elements");
            }

            var idx = (int)context.Args[1].EvaluateDouble(context.Thread);

            if (idx < 0 || idx >= container.Instances.Count)
            {
                throw new RunException($"{nameof(GetOffset)} index must be between 0 and {container.Instances.Count}");
            }

            return container.Instances[idx].Offset;
        }

        private static void AddChildren(MethodContext context, Container container, int startAt = 0)
        { 
            for (int i = startAt; i < context.Args.Count; ++i)
            {
                var child = context.Args[i].Evaluate(context.Thread);
                var source = child;
                var offset = 0D;

                // Use parameters?
                if (context.Args[i].Type == AstType.Object)
                {
                    var elements = (child as Dictionary<string,object>)!;
                    if (elements.ContainsKey(Constants.TimeKey))
                    {
                        offset = Convert.ToDouble(elements[Constants.TimeKey]);
                    }
                    if (!elements.ContainsKey(Constants.SampleKey) && !elements.ContainsKey(Constants.ContainerKey))
                    {
                        throw new RunException("Sample or track not provided");
                    }
                    source = elements.ContainsKey(Constants.SampleKey) ?
                        elements[Constants.SampleKey] :
                        elements[Constants.ContainerKey];
                }

                if (!(source is SongElement songElement))
                {
                    throw new RunException($"Cannot add a non-song element");
                }

                container.Add(songElement, offset);
            }
        }
    }
}

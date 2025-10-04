using Irony.Parsing;
using MW.Audio;
using MW.Helpers;
using MW.Parsing;
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
        [Function(name: "t", astType: AstType.CSObject, description: "Create track")]
        public static Container CreateContainer(MethodContext context)
        {
            var container = new Container();
            for (int i = 0; i < context.Args.Count; ++i)
            {
                var child = Func.ConvertAndValidate<SongElement>(context.Args[i], context.Thread);

                switch (child)
                {
                    case Sample sampleChild:
                        {
                            container.Instances.Add(new Instance(sampleChild));
                            break;
                        }
                    case Container containerChild:
                        {
                            container.Containers.Add(containerChild);
                            break;
                        }
                    default:
                        {
                            throw new RunException($"Arguments must be of type {nameof(AudioSource)} or {nameof(Container)}");
                        }
                }
            }

            return container;
        }
    }
}

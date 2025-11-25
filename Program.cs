using Pulumi;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using System.Collections.Generic;
using System;
using Kubernetes = Pulumi.Kubernetes;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Apps.V1;

return await Pulumi.Deployment.RunAsync( () => {
    var k8sProvider = new Kubernetes.Provider("provider", new Kubernetes.ProviderArgs {
        Namespace = "test"
    } );

    var ns = new Namespace( "ns", new NamespaceArgs {
        Metadata = new ObjectMetaArgs {
            Name = "test"
        }
    }, new CustomResourceOptions { Provider = k8sProvider } );

    var appLabels = new InputMap<string> {
        { "app", "nginx" },
        { "version", "2" },
    };
    
    _ = new StatefulSet( "my-stateful-set", new StatefulSetArgs {
        Metadata = new ObjectMetaArgs {
            Name = "test",
        },
        Spec = new StatefulSetSpecArgs {
            Selector = new LabelSelectorArgs {
                MatchLabels = appLabels,
            },
            Replicas = 1,
            UpdateStrategy = new StatefulSetUpdateStrategyArgs {
                Type = "RollingUpdate"
            },
            Template = new PodTemplateSpecArgs {
                Metadata = new ObjectMetaArgs {
                    Labels = {
                        appLabels
                    },
                },
                Spec = new PodSpecArgs {
                    Containers = new ContainerArgs {
                        Name = "nginx",
                        Image = "nginx",
                    }
                },
            },
        },
    }, new CustomResourceOptions {
        Provider = k8sProvider,
        ReplaceOnChanges = [ "*" ],
        DeleteBeforeReplace = true,
        CustomTimeouts = new CustomTimeouts {
            Create = TimeSpan.FromMinutes( 1 ),
            Update = TimeSpan.FromMinutes( 1 )
        }
    } );

    // var deployment = new Pulumi.Kubernetes.Apps.V1.Deployment( "nginx", new DeploymentArgs {
    //     Spec = new DeploymentSpecArgs {
    //         Selector = new LabelSelectorArgs {
    //             MatchLabels = appLabels
    //         },
    //         Replicas = 1,
    //         Template = new PodTemplateSpecArgs {
    //             Metadata = new ObjectMetaArgs {
    //                 Labels = appLabels
    //             },
    //             Spec = new PodSpecArgs {
    //                 Containers =
    //                 {
    //                     new ContainerArgs
    //                     {
    //                         Name = "nginx",
    //                         Image = "nginx",
    //                         Ports =
    //                         {
    //                             new ContainerPortArgs
    //                             {
    //                                 ContainerPortValue = 80
    //                             }
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // } );

    // // export the deployment name
    // return new Dictionary<string, object?> {
    //     [ "name" ] = deployment.Metadata.Apply( m => m.Name )
    // };
} );

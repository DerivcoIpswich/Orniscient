# Orniscient
Orniscient provides an interface for visualising user grains in an Orleans cluster or meta-cluster. Grains can be filtered based on pre-defined public properties, or identifiers such as GUIDs or integer keys, in addition to concrete type definitions.

Reflection can also be used to invoke methods on active (or in future inactive) grains.

## PLEASE NOTE: Status
Orniscient is still in early development and requires a post-1.2 version of Orleans to use, as it relies on a PR submitted to enable the introspection of grain data. From Orleans 1.3 onwards, Orniscient will work with any Orleans cluster.

## Quick Start (WIP)
1. Install the Orniscient proxy into your cluster by stating the following into package manager:

        Install-Package Derivco.Orniscient.Proxy -Pre
2. If not automatically added, manually add this snippet to your Orleans silo configuration file:

        <OrleansConfiguration xmlns="urn:orleans">
            <Globals>
                <BootstrapProviders>
                    <Provider Type="Derivco.Orniscient.Proxy.BootstrapProviders.OrniscientFilterInterceptor" 
                        Name="OrniscientFilterInterceptor"/>
                </BootstrapProviders>
            </Globals>
        </OrleansConfiguration>
        
3. Download the Orniscient Viewer project, configure it to point at the cluster, build it, and execute it.

        More specific details to follow.

## Quick Links
* [Orniscient Chat](http://gitter.im/DerivcoIpswich/Orniscient/)
* [Orniscient Documentation](http://DerivcoIpswich.github.io/Orniscient/)

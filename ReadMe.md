**Project Description**
With re-linq, it's now easier than ever to create full-featured LINQ providers.

Yes, you've heard that before. But re-linq is the real thing: it is **used by Entity Framework 7 and NHibernate**.
* Instead of the IQueryable expression tree, re-linq gives you an abstract syntax tree that resembles the original LINQ query expression (the one using the **from**, **where** etc. keywords). 
	* This works even when the original query was not a query expression, but a sequence of method invocations of Where(), Select() etc.
	* You can easily extend or modify quieries using this representation in your application, directly or via a Specification framework.
* In the process, re-linq gets rid of everything that IQueryable puts between you and your desired target query language:
	* The structure is simplified (e.g., SelectMany is transformed back to multiple **from**-sources).
	* Transparent identifiers are removed.
	* Sub-queries are identified and can be handled as such.
	* Expressions are evaluated up-front wherever possible.
	* Special method calls inserted by Visual Basic are "normalized" so that they are handled implicitly, except if you choose to handle them differently. 
* re-linq handles even the most complex LINQ query expression, including joins, groups and nested queries.
* re-linq gives you the tools to easily create your own provider, including:
	* A set of visitors that you just implement for your transformation.
	* A registry for translating methods to specific expressions in the target query language (like string methods, or ICollection.Contains).
	* A mechanism to specify eager fetching for related objects or collections that will automatically create the required queries.
* re-linq is completely agnostic of the target query language. You can use it to create any dialect of SQL as well as other query languages such as XQL or Entity SQL. For instance, the [NHibernate](http://nhforge.org/) project [uses re-linq](https://nhibernate.svn.sourceforge.net/svnroot/nhibernate/trunk/nhibernate/src/NHibernate/Linq/) to [create HQL ASTs](http://blogs.imeta.co.uk/sstrong/Tags/Linq/default.aspx).

Depending on the differences between LINQ and the target query language you want to address, there is probably still a major piece of work ahead of you. But with re-linq this is about as simple as it can be: just what you thought was ahead of you before you discovered the internals of IQueryable, and then some more help. 

There's one more potential advantage: The remaining effort is probably mostly in the semantic transformation from LINQ to the target query language, especially in cases where the target language does not support certain LINQ constructs (like sub-queries or LINQ group joins). Also, certain optimizations need to be made, e.g. in order to avoid the [select n+1 problem](http://www.google.com/search?q=select+n%2B1) via [eager fetching](http://groups.google.com/group/nhibernate-development/browse_thread/thread/c05d2c7ea7233340/64032597075c7130?lnk=gst#64032597075c7130). In many cases, the necessary transformations are similar between various target languages. re-linq allows you to do these transformations in its own query model representation, before you actually translate to the target language. These transformations can be shared between different LINQ providers via the re-motion contrib repository.

re-linq is part of the re-motion project. See the [project home page](http://www.re-motion.org) for more interesting libraries, such as [re-mix](https://github.com/re-motion/Remix/).

**Ressources**
* [re-motion team blog](http://www.re-motion.org/blogs/team) featuring [announcements and a general discussion of re-linq](http://www.re-motion.org/blogs/team/category/re-linq)
* [Fabian's Mix](http://www.re-motion.org/blogs/mix) with [details about the implementation of re-linq](http://www.re-motion.org/blogs/mix/category/re-linq)
* [Whitepaper](http://www.re-motion.org/download/re-linq.pdf) about the ideas behind re-linq
* [End-to-end sample and article](http://www.codeproject.com/KB/linq/relinqish_the_pain.aspx) at codeproject.com 
* re-motion [discussion list](http://groups.google.com/group/re-motion-users)
* re-motion [issue tracker](https://re-motion.atlassian.net/projects/RMLNQ)

**Related Projects**
* [https://github.com/re-motion/Relinq-SqlBackend](https://github.com/re-motion/Relinq-SqlBackend)
* [https://github.com/re-motion/Relinq-EagerFetching](https://github.com/re-motion/Relinq-EagerFetching)

**Sources**
The most current source code of re-linq is available on GitHub:
* [https://github.com/re-motion/Relinq](https://github.com/re-motion/Relinq)

**Binaries**
The re-linq releases are distributed as NuGet packages:
* [https://www.nuget.org/packages/Remotion.Linq/](https://www.nuget.org/packages/Remotion.Linq/)
* [https://www.nuget.org/packages/Remotion.Linq.Development/](https://www.nuget.org/packages/Remotion.Linq.Development/)
If you also want the documentation, you can grab the equivalent releases from
[https://www.myget.org/gallery/re-motion-release](https://www.myget.org/gallery/re-motion-release).

**Versioning**
Starting with version 2.0, the releases follow Semantic Versioning ([http://semver.org/](http://semver.org/))

**License**
The Remotion.Linq.dll (the "Frontend") is licensed under the Apache Software License 2.0.

**Contributing**
We have official contribution practices documented: [Contributing to re-linq](https://github.com/re-motion/Relinq/wiki/Contributing)
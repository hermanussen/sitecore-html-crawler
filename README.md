sitecore-html-crawler
=====================

Use this computed field implementation to index Sitecore pages by loading the actual page and extracting the content.

## Motivation

The reason you would want to do this, is because of how content composition in the page editor works. You can add components to a page that reference separate Sitecore items as associated content (a.k.a. datasources). But the content of those referenced items is indexed separately and therefore difficult to search for using Sitecore.ContentSearch (Sitecore 7 search).

## How it works

That's why this crawler adds a computed field to your index that contains the actually rendered page's content. The page is requested from the server and all html tags are stripped so only the relevant content from the &lt;body /&gt; element is left.

## Installation

1. Download the latest package from https://github.com/hermanussen/sitecore-html-crawler/tree/master/SitecorePackages
2. Install the package using the installation wizard

## Limitations

- This doesn't really work well with wildcard items, because they can only be indexed once; not for every item that you want to display on the wildcard item's page. You would need to change the actual indexer to support that, or handle the difficulty of it in your search query.
- I've only tested this with Lucene. I think changing the configuration a little could make it work for other providers such as Solr.

## License

Copyright (C) 2014 Robin Hermanussen

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see http://www.gnu.org/licenses/.
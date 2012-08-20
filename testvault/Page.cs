using System;
using System.Text;
using System.Linq;
using System.Security;
using System.Collections.Generic;
using TestVault.Data;

namespace TestVault
{

    public class Page
    {

        public static KeyValuePair<string,string> Pair(string a, string b)
        {
            return new KeyValuePair<string, string>( a, b );
        }

        public string SelfLink(string text, params KeyValuePair<string,string>[] args)
        {
            var pl = new Dictionary<string,string>();
            foreach ( var p in Params.Keys ){
                pl[p] = Params[p];
            }
            foreach ( var x in args ){
                pl[x.Key] = x.Value;
            }
            var href = new List<string>();
            foreach ( var p in pl.Keys ){
                var pn = SecurityElement.Escape(p);
                var pv = SecurityElement.Escape(pl[p]);
                var to = string.Format("{0}={1}",pn,pv);
                href.Add(to);
            }
            var linkhref = string.Join("&amp;",href.ToArray());

            return Tag(string.Format("a,href:?{0}",linkhref),text);
        }

        public static string Tag(string tag, params string[] content)
        {
            var attribs = tag.Split(',');
            var alist = attribs.Skip(1).ToArray();

            var other_attribs = new Dictionary<string,string>();
            foreach (var a in alist)
            {
                var pair = a.Split(':');

                if (SecurityElement.IsValidAttributeName(pair [0]))
                {
                    if (SecurityElement.IsValidAttributeValue(pair [1]))
                    {
                        other_attribs [pair [0]] = pair [1];
                    }
                }

            }

            tag = attribs [0];

            var tagid = tag.Split('#');

            var id = "";
            if (tagid.Length == 2)
            {
                if ( SecurityElement.IsValidAttributeValue(tagid[1]) )
                    id = tagid [1];
            }

            var tagclass = tagid [0].Split('.');
            var tn = tagclass [0];

            var classes = tagclass.Skip(1);


            var sb = new StringBuilder();
            sb.AppendFormat("<{0}", tn.ToLower());

            if (classes.Count() > 0)
            {
                foreach (var c in classes)
                {
                    if ( SecurityElement.IsValidAttributeValue( c ))
                        sb.AppendFormat(" class=\"{0}\"", c);
                }
            }

            foreach (var a in other_attribs.Keys)
            {
                sb.AppendFormat(" {0}=\"{1}\"", a, other_attribs[a] );
            }

            if (!string.IsNullOrEmpty(id))
            {
                sb.AppendFormat(" id=\"{0}\"",id);
            }

            if (content.Length == 0)
            {
                sb.Append("/");
            } else
            {
                sb.Append(">");
                sb.AppendFormat( string.Join(" ", content ) );
                sb.AppendFormat("</{0}",tn.ToLower());
            }

            sb.Append(">");
            return sb.ToString();
        }

        public Page()
        {
            Params = new Dictionary<string, string>();
        }

        public string Title { get; set; }

        public string Heading { get; set; }

        public string StyleSheet { get; set; }

        public Dictionary<string,string> Params { get; set; }

        public virtual IEnumerable<string> Sections
        {
            get
            {
                return new string[] { };
            }
        }

        public override string ToString()
        {
            var css = "";
            if (!String.IsNullOrEmpty(StyleSheet))
            {
                css = Tag(string.Format("link,href:{0},rel:stylesheet,type:text/css", StyleSheet));
            }

            var head = Tag("head",
                           Tag("title", Title), css);


            var preamble = Tag("div",
                               Tag("div.header#top,align:right",
                               Tag("small", String.Format("Test Vault on {0}", System.Environment.MachineName))),
                               Tag("h1", Heading)
            );

            var parts = new List<string> { preamble };
           
            parts.AddRange( Sections );

            return Tag("html", head,
                       Tag("body", parts.ToArray() ) );
        }
    }
}


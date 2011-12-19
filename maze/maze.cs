using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace maze
{
    class maze
    {
        public class node
            :IComparable<node>
        {
            public struct point
            {
                public int x, y;
                public char? to_arrow_char()
                {
                    if (this == point.up) return '↑';
                    if (this == point.right) return '→';
                    if (this == point.bottom) return '↓';
                    if (this == point.left) return '←';
                    return null;
                }
                static public bool operator ==(point a, point b)
                { return a.x == b.x && a.y == b.y; }
                static public bool operator !=(point a, point b)
                { return !(a == b); }
                static public point operator +(point a, point b)
                { return new point() { x = a.x + b.x, y = a.y + b.y }; }
                static public point operator -(point a, point b)
                { return new point() { x = a.x - b.x, y = a.y - b.y }; }
                static public IEnumerable<point> enum_around(point p)
                {
                    yield return p + point.up;
                    yield return p + point.right;
                    yield return p + point.bottom;
                    yield return p + point.left;
                }
                static public point up { get { return new point() { x = 0, y = -1 }; } }
                static public point right { get { return new point() { x = 1, y = 0 }; } }
                static public point bottom { get { return new point() { x = 0, y = 1 }; } }
                static public point left { get { return new point() { x = -1, y = 0 }; } }
            }

            public point position { get; set; }
            public float cost { get; set; }
            public LinkedList<node> connected_nodes { get; set; }

            public node()
            {
                position = new point();
                connected_nodes = new LinkedList<node>();
                cost = float.MaxValue;
            }

            public node before
            { get { return connected_nodes.Min(); } }

            public int CompareTo(node other)
            {
                var d = cost - other.cost;

                if (d > 0f)
                    return 1;
                else if (d < 0f)
                    return -1;
                return 0;
            }
        }

        public const string result_ext = ".result.txt";

        public char[] maze_charactor_way { get; set; }
        public char[] maze_charactor_block { get; set; }
        public char[] maze_charactor_start { get; set; }
        public char[] maze_charactor_end { get; set; }

        protected string[] maze_text;
        protected List<node> nodes;
        protected node start_node, end_node;
        protected Stack<node> shortest_path;

        public int maze_width { get { return maze_text.First().Length; } }
        public int maze_height { get { return maze_text.Length; } }

        public maze()
        { reset(); }

        public void reset()
        {
            maze_charactor_way = new char[] { '□', '.' };
            maze_charactor_block = new char[] { '■', '#' };
            maze_charactor_start = new char[] { 'Ｓ', 'S' };
            maze_charactor_end = new char[] { 'Ｇ', 'G' };
            maze_text = null;
            start_node = end_node = null;
            nodes = null;
            shortest_path = null;
        }

        public void load(string in_file)
        {
            var maze_text_ = System.IO.File.ReadAllText(in_file);
            maze_text = load_replace(maze_text_).Split(new string[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            System.Diagnostics.Debug.WriteLine("\n<load>\n" + string.Join(Environment.NewLine, maze_text));
        }

        protected string load_replace(string t)
        {
            Action<char[]> f = c =>
            {
                foreach (var r in c.Reverse().Take(maze_charactor_way.Length - 1))
                    t = t.Replace(r, c[0]);
            };
            f(maze_charactor_way);
            f(maze_charactor_block);
            f(maze_charactor_start);
            f(maze_charactor_end);
            return t;
        }

        public void generate_nodes()
        {
            generate_nodes_initialize();
            generate_nodes_set_connected_nodes();
        }

        protected void generate_nodes_initialize()
        {
            nodes = new List<node>();

            var width = maze_width;
            var height = maze_height;

            for (var y = 0; y < height; ++y)
                for (var x = 0; x < width; ++x)
                {
                    if (maze_text[y][x] == maze_charactor_block[0])
                        continue;

                    var n = new node() { position = new node.point() { x = x, y = y } };
                    nodes.Add(n);

                    if (maze_text[y][x] == maze_charactor_start[0])
                        start_node = n;
                    else if (maze_text[y][x] == maze_charactor_end[0])
                        end_node = n;
                }
        }

        protected void generate_nodes_set_connected_nodes()
        {
            foreach (var n in nodes)
            {
                foreach (var p in node.point.enum_around(n.position))
                {
                    var c = nodes.Find(t => t.position == p);
                    if (c == null)
                        continue;
                    n.connected_nodes.AddLast(c);
                }
            }
        }

        public void find()
        {
            start_node.cost = 0f;
            find(start_node);
            shortest_path = new Stack<node>();
            for (var n = end_node; n != start_node; n = n.before)
                shortest_path.Push(n);
        }

        protected void find(node n)
        {
            var next_cost = n.cost + 1f;
            foreach (var t in n.connected_nodes)
            {
                if (t.cost > next_cost)
                {
                    t.cost = next_cost;
                    find(t);
                }
            }
        }

        public string save_string
        {
            get
            {
                var width_plus = maze_width + Environment.NewLine.Length;

                var result_maze_chars = string.Join(Environment.NewLine, maze_text).ToCharArray();
                var current_position = shortest_path.Peek().position;

                foreach (var n in shortest_path)
                {
                    var direction = n.position - current_position;
                    var char_position = current_position.y * width_plus + current_position.x;
                    var direction_char = direction.to_arrow_char() ?? '＊';
                    result_maze_chars[char_position] = direction_char;
                    current_position = n.position;
                }

                return string.Join(string.Empty, result_maze_chars);
            }
        }

        public void save(string out_file)
        {
            var s = save_string;
            System.IO.File.WriteAllText(out_file, s, Encoding.Unicode);
            System.Diagnostics.Debug.WriteLine("\n<save>\n" + s);
        }

        static public void find_path(string in_file)
        {
            var out_file = in_file + result_ext;
            var m = new maze();
            m.load(in_file);
            m.generate_nodes();
            m.find();
            m.save(out_file);
        }
    }
}

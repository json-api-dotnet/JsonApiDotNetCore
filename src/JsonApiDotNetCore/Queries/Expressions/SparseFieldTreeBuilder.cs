using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

internal sealed class SparseFieldTreeBuilder
{
    private readonly SparseFieldTreeNode _root = new(null);

    public static SparseFieldTreeBuilder FromExpression(SparseFieldSetExpression sparseFieldSet)
    {
        var builder = new SparseFieldTreeBuilder();

        foreach ((ResourceFieldAttribute field, SparseFieldSetElementExpression element) in sparseFieldSet.Elements)
        {
            builder._root.AddChild(field, SparseFieldTreeNode.FromElement(element));
        }

        return builder;
    }

    public void IncludeChain(IReadOnlyList<ResourceFieldAttribute> chain)
    {
        if (chain.Count > 0)
        {
            _root.IncludeSubChain(chain, 0);
        }
    }

    public void ExcludeChain(IReadOnlyList<ResourceFieldAttribute> chain)
    {
        if (chain.Count > 0)
        {
            _root.ExcludeSubChain(chain, 0);
        }
    }

    public SparseFieldSetExpression ToExpression()
    {
        return SparseFieldSetExpression.Create(_root.ToChildrenDictionary());
    }

    private sealed class SparseFieldTreeNode(ResourceFieldAttribute? field)
    {
        private readonly Dictionary<ResourceFieldAttribute, SparseFieldTreeNode> _children = [];

        private readonly ResourceFieldAttribute? _field = field;
        private bool _selectAllChildren;

        private bool IsEmpty => !_selectAllChildren && _children.Count == 0 && _field is AttrAttribute { IsCompound: true };

        public void AddChild(ResourceFieldAttribute fieldToAdd, SparseFieldTreeNode node)
        {
            _children[fieldToAdd] = node;
        }

        public static SparseFieldTreeNode FromElement(SparseFieldSetElementExpression element)
        {
            var node = new SparseFieldTreeNode(element.Field)
            {
                _selectAllChildren = element.Children.Count == 0 && element.Field is AttrAttribute { IsCompound: true }
            };

            foreach ((ResourceFieldAttribute childField, SparseFieldSetElementExpression child) in element.Children)
            {
                node._children[childField] = FromElement(child);
            }

            return node;
        }

        public void IncludeSubChain(IReadOnlyList<ResourceFieldAttribute> chain, int index)
        {
            if (index >= chain.Count)
            {
                // This node is the leaf of the included chain.
                _selectAllChildren = _field is AttrAttribute { IsCompound: true };
                _children.Clear();
                return;
            }

            if (_selectAllChildren)
            {
                // Parent already selects all child fields, so deeper path is redundant.
                return;
            }

            ResourceFieldAttribute childField = chain[index];

            if (!_children.TryGetValue(childField, out SparseFieldTreeNode? childNode))
            {
                childNode = new SparseFieldTreeNode(childField)
                {
                    _selectAllChildren = index == chain.Count - 1 && childField is AttrAttribute { IsCompound: true }
                };

                _children[childField] = childNode;
            }

            childNode.IncludeSubChain(chain, index + 1);
        }

        public void ExcludeSubChain(IReadOnlyList<ResourceFieldAttribute> chain, int index)
        {
            if (_selectAllChildren && _field is AttrAttribute { CompoundType: not null } compoundAttr)
            {
                ExpandAllChildren(compoundAttr.CompoundType);
            }

            ResourceFieldAttribute childField = chain[index];

            if (!_children.TryGetValue(childField, out SparseFieldTreeNode? childNode))
            {
                return;
            }

            if (index == chain.Count - 1)
            {
                _children.Remove(childField);
                return;
            }

            childNode.ExcludeSubChain(chain, index + 1);

            if (childNode.IsEmpty)
            {
                _children.Remove(childField);
            }
        }

        private void ExpandAllChildren(CompoundType compoundType)
        {
            _selectAllChildren = false;

            foreach (AttrAttribute attr in compoundType.Attributes)
            {
                if (!attr.IsViewBlocked() && !_children.ContainsKey(attr))
                {
                    _children[attr] = new SparseFieldTreeNode(attr)
                    {
                        _selectAllChildren = attr.IsCompound
                    };
                }
            }
        }

        public ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> ToChildrenDictionary()
        {
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Builder builder =
                ImmutableDictionary.CreateBuilder<ResourceFieldAttribute, SparseFieldSetElementExpression>();

            foreach ((ResourceFieldAttribute childField, SparseFieldTreeNode childNode) in _children)
            {
                builder.Add(childField, childNode.ToElement());
            }

            return builder.ToImmutable();
        }

        private SparseFieldSetElementExpression ToElement()
        {
            return _selectAllChildren || _children.Count == 0
                ? new SparseFieldSetElementExpression(_field!)
                : new SparseFieldSetElementExpression(_field!, ToChildrenDictionary());
        }
    }
}

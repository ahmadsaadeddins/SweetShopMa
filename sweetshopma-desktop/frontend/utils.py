"""
Utility functions for SweetShopMa Desktop Application.
"""

import os
from jinja2 import Environment, FileSystemLoader


def render_template(template_name, title="SweetShopMa", context=None):
    """
    Render HTML template using Jinja2.
    
    Args:
        template_name (str): Name of the template file
        title (str): Page title
        context (dict): Additional context variables
        
    Returns:
        str: Rendered HTML
    """
    # Get template directory
    template_dir = os.path.join(
        os.path.dirname(__file__),
        'templates'
    )
    
    # Create Jinja2 environment
    env = Environment(
        loader=FileSystemLoader(template_dir),
        autoescape=True
    )
    
    # Load and render template
    template = env.get_template(template_name)
    
    # Merge context
    template_context = {
        'title': title,
        'app_name': 'SweetShopMa',
        'version': '1.0.0',
    }
    
    if context:
        template_context.update(context)
    
    html = template.render(**template_context)
    
    return html


def get_asset_path(asset_path):
    """
    Get the full path to a static asset.
    
    Args:
        asset_path (str): Relative path to asset
        
    Returns:
        str: Full path to asset
    """
    return os.path.join(
        os.path.dirname(__file__),
        'static',
        asset_path
    )


def read_asset(asset_path):
    """
    Read the contents of a static asset file.
    
    Args:
        asset_path (str): Relative path to asset
        
    Returns:
        str: File contents
    """
    full_path = get_asset_path(asset_path)
    
    try:
        with open(full_path, 'r', encoding='utf-8') as f:
            return f.read()
    except FileNotFoundError:
        return f"Asset not found: {asset_path}"
    except Exception as e:
        return f"Error reading asset: {e}"

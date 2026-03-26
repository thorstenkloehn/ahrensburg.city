import xml.etree.ElementTree as ET
from datetime import datetime
import os

def convert_mediawiki_to_meincms(input_file, output_file, tenant_id="doc"):
    tree = ET.parse(input_file)
    root = tree.getroot()

    # Try to detect namespace from the root tag
    # Example: {http://www.mediawiki.org/xml/export-0.10/}mediawiki
    if root.tag.startswith('{'):
        namespace = root.tag.split('}')[0].strip('{')
        ns = {'mw': namespace}
    else:
        ns = None

    array_of_wiki_artikel = ET.Element('ArrayOfWikiArtikel')
    array_of_wiki_artikel.set('xmlns:xsi', 'http://www.w3.org/2001/XMLSchema-instance')
    array_of_wiki_artikel.set('xmlns:xsd', 'http://www.w3.org/2001/XMLSchema')

    article_id_counter = 1
    version_id_counter = 1

    # If no namespace, we don't use the prefix
    page_tag = 'mw:page' if ns else 'page'
    title_tag = 'mw:title' if ns else 'title'
    revision_tag = 'mw:revision' if ns else 'revision'
    timestamp_tag = 'mw:timestamp' if ns else 'timestamp'
    text_tag = 'mw:text' if ns else 'text'

    for page in root.findall(page_tag, ns):
        title = page.find(title_tag, ns).text
        
        wiki_artikel = ET.SubElement(array_of_wiki_artikel, 'WikiArtikel')
        ET.SubElement(wiki_artikel, 'Id').text = str(article_id_counter)
        ET.SubElement(wiki_artikel, 'TenantId').text = tenant_id
        ET.SubElement(wiki_artikel, 'Slug').text = title
        
        versionen = ET.SubElement(wiki_artikel, 'Versionen')
        
        # In MediaWiki export, there might be multiple revisions, but usually we just take them all
        for revision in page.findall(revision_tag, ns):
            timestamp_str = revision.find(timestamp_tag, ns).text
            # MediaWiki timestamp is usually YYYY-MM-DDTHH:MM:SSZ
            
            text_element = revision.find(text_tag, ns)
            markdown_inhalt = text_element.text if text_element is not None else ""
            
            version = ET.SubElement(versionen, 'WikiArtikelVersion')
            ET.SubElement(version, 'VersionNummer').text = str(version_id_counter)
            ET.SubElement(version, 'TenantId').text = tenant_id
            ET.SubElement(version, 'MarkdownInhalt').text = markdown_inhalt
            ET.SubElement(version, 'HtmlInhalt') # Empty as per requirement
            ET.SubElement(version, 'Zeitpunkt').text = timestamp_str
            ET.SubElement(version, 'Kategorie')
            ET.SubElement(version, 'WikiArtikelId').text = str(article_id_counter)
            
            version_id_counter += 1
            
        article_id_counter += 1

    # Write to file with XML declaration
    output_tree = ET.ElementTree(array_of_wiki_artikel)
    if hasattr(ET, 'indent'):
        ET.indent(output_tree, space="  ", level=0)
    output_tree.write(output_file, encoding='utf-8', xml_declaration=True)

if __name__ == "__main__":
    convert_mediawiki_to_meincms('migration/doc.xml', 'migration/thorstendoc.xml', 'doc')
    print("Conversion completed: migration/thorstendoc.xml")

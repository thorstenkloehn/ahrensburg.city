import fs from 'fs'
import path from 'path'
import Link from 'next/link'

export async function getStaticProps() {
  const blogDirectory = path.join(process.cwd(), '_blog')
  const fileNames = fs.readdirSync(blogDirectory)

  const allPostsData = fileNames.map(fileName => {
    const filePath = path.join(blogDirectory, fileName)
    const fileContents = fs.readFileSync(filePath, 'utf8')

    // Hier können Sie die Dateiinhalte verarbeiten...

    return {
      id: fileName.replace(/\.md$/, ''),
      content: fileContents
    }
  })

  return {
    props: {
      allPostsData
    }
  }
}

export default function Page({ allPostsData }) {
  return (
    <div>
    {allPostsData.map(({ id, content }) => (
      <div key={id}>
        <h2>{id}</h2>
        <p>{content}</p>
        <Link href={`/Blog/${id}`}>Lesen Sie mehr</Link>
      </div>
    ))}
  </div>
  )
}
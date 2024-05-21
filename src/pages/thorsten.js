import fs from 'fs'
import path from 'path'
import Link from 'next/link'

export async function getStaticProps() {
  const blogDirectory = path.join(process.cwd(), '_blog')
  const fileNames = fs.readdirSync(blogDirectory)

  const allPostsData = fileNames.map(fileName => {
    const filePath = path.join(blogDirectory, fileName)
    const fileContents = fs.readFileSync(filePath, 'utf8')
    const creationDate = fs.statSync(filePath).birthtime

    // Hier können Sie die Dateiinhalte verarbeiten...

    return {
      id: fileName.replace(/\.md$/, ''),
      content: fileContents,
      date: creationDate.toISOString() // Konvertieren Sie das Date-Objekt in einen String
    }
  })

  // Sortieren Sie die Posts nach Datum, neueste zuerst
  allPostsData.sort((a, b) => new Date(b.date) - new Date(a.date))

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
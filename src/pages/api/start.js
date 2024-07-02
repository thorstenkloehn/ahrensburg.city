export default function handler(req, res) {
  // Ersetzen Sie 'IHR_GEHEIMER_SCHLÜSSEL' mit Ihrem tatsächlichen geheimen Schlüssel
  const geheimerSchlüssel = 'Test';

  // Beispiel: Überprüfung des Schlüssels im Query-String
  const schlüsselImQueryString = req.query.schlüssel;

  // Beispiel: Überprüfung des Schlüssels im Header
  // const schlüsselImHeader = req.headers['x-api-schlüssel'];

  if (schlüsselImQueryString !== geheimerSchlüssel) {
    // Wenn der Schlüssel nicht übereinstimmt, senden Sie eine 401 Unauthorized Antwort
    return res.status(401).json({ error: 'Unbefugter Zugriff' });
  }

  // Wenn der Schlüssel übereinstimmt, fahren Sie mit der ursprünglichen Antwort fort
  res.status(200).json({ text: 'Thomas' });
}
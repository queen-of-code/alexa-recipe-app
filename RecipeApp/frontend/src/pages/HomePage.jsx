import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

const features = [
  {
    title: 'Store & Retrieve Recipes',
    description: 'Edit, delete, and categorize your own personal recipes using the web ui.',
  },
  {
    title: 'Backup to the Cloud',
    description:
      'Ensure you never lose a precious family recipe again by backing up to the cloud. Keep all your data in sync, no matter what the device.',
  },
  {
    title: 'Interact With Alexa',
    description:
      "Now you can ask, \u2018Alexa, read me the next step!\u2019 and receive verbal instructions. No more stained recipe books!",
  },
  {
    title: 'Plan Weekly Meals',
    description:
      'Simply provide a few key ingredients, and it will generate a weekly meal plan - complete with ingredient shopping list.',
  },
]

export default function HomePage() {
  const user = useAuth()

  return (
    <>
      <section className="bg-gradient-to-r from-violet-700 to-indigo-800 text-white py-20 px-8 text-center">
        <h1 className="text-4xl md:text-5xl font-bold mb-4">Alexa Recipe App</h1>
        <p className="text-lg md:text-xl text-violet-100 max-w-2xl mx-auto mb-8">
          This is your customized recipe library! There are many amazing functions supported, such as:
        </p>
        {!user && (
          <Link
            to="/login"
            className="inline-block bg-white text-violet-700 font-semibold px-6 py-3 rounded-lg hover:bg-violet-50 transition-colors"
          >
            Get Started
          </Link>
        )}
      </section>

      <section className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-4xl mx-auto my-12 px-4">
        {features.map((feature) => (
          <div key={feature.title} className="bg-white rounded-xl shadow p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-2">{feature.title}</h2>
            <p className="text-gray-600 text-sm leading-relaxed">{feature.description}</p>
          </div>
        ))}
      </section>
    </>
  )
}

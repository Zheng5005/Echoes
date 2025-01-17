import Navbar from "./components/Navbar"
import Home from "./pages/Home"
import Signup from "./pages/Signup"
import Login from "./pages/Login"
import Settings from "./pages/Settings"
import Profile from "./pages/Profile"
import {Routes, Route} from "react-router-dom"

function App() {
  return (
    <>
      <div>
        <Navbar />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/signup" element={<Signup />} />
          <Route path="/login" element={<Login />} />
          <Route path="/settings" element={<Settings />} />
          <Route path="/profile" element={<Profile />} />
        </Routes>
      </div>
    </>
  )
}

export default App

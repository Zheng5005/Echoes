import mongoose from "mongoose"
import dotenv from "dotenv"

dotenv.config()
async function connectDB() {
    try {
        const conn = await mongoose.connect(process.env.MONGODB_URI)
        console.log(`MongoDB connected: ${conn.connection.host}`)
    } catch (error) {
        console.log("MongoDB connection error:", error);
    }   
}

export default connectDB
import {create} from "zustand"
import toast from "react-hot-toast"
import {axiosInstance} from '../lib/axios.js'

const useChatStore = create((set, get) => ({
    messages: [],
    users: [],
    selectedUser: null,
    isUsersLoading: false,
    isMessagesLoading: false,

    getUsers: async () => {
        set({isUsersLoading: true})
        try {
            const res = await axiosInstance.get("messages/users")
            set({users: res.data})
        } catch (error) {
            console.log("Error in get Users", error);
            toast.error(error.message)
        } finally {
            set({isUsersLoading: false})
        }
    },

    getMessages: async (userId) => {
        set({isMessagesLoading: true})
        try {
            const res = await axiosInstance.get(`messages/${userId}`)
            set({messages: res.data})
        } catch (error) {
            console.log("Error in get Messages", error);
            toast.error(error.message)
        } finally {
            set({isMessagesLoading: false})
        }
    },

    sendMessage: async (messageData) => {
        const {selectedUser, messages} = get()
        try {
            const res = await axiosInstance.post(`/messages/send/${selectedUser._id}`, messageData)
            set({messages: [...messages, res.data]})
        } catch (error) {
            console.log("Error in send Messages", error);
            toast.error(error.message)
        }
    },

    //Optimaze it
    setSelectedUser: (selectedUser) => set({selectedUser})
}))

export default useChatStore
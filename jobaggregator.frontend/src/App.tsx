import React, { lazy, Suspense } from "react";
import { Routes, Route } from "react-router-dom";
import { ThemeProvider } from "@mui/material/styles";
import theme from "./theme";
import { Helmet } from "react-helmet";
import { Box, CircularProgress } from "@mui/material";
import LoginPage from "./pages/LoginPage";
import ProfilePage from "./pages/ProfilePage";
import ProtectedRoute from "./components/ProtectedRoute";
import { AuthProvider } from "./services/AuthContext";
import NavBar from "./components/NavBar";
import Footer from "./components/Footer";
import RegisterPage from "./pages/RegisterPage";

const Home = lazy(() => import("./pages/Home"));
const SearchResults = lazy(() => import("./pages/SearchResults"));
const JobDetail = lazy(() => import("./pages/JobDetail"));

const App: React.FC = () => {
  return (
    <>
      <Helmet>
        <title>HledačPráce - Najděte si svoji ideální práci</title>
        <meta
          name="description"
          content="Vyhledávejte mezi tisíci pracovních nabídek na jednom místě."
        />
      </Helmet>
      <AuthProvider>
        <ThemeProvider theme={theme}>
          <Suspense
            fallback={
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  height: "100vh",
                }}
              >
                <CircularProgress size={60} />
              </Box>
            }
          >
            <NavBar />
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/search" element={<SearchResults />} />
              <Route path="/job/:id" element={<JobDetail />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route
                path="/profile"
                element={
                  <ProtectedRoute>
                    <ProfilePage />
                  </ProtectedRoute>
                }
              />
            </Routes>
            <Footer />
          </Suspense>
        </ThemeProvider>
      </AuthProvider>
    </>
  );
};

export default App;

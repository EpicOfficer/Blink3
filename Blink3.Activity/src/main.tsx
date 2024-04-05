import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import {AuthProvider} from './contexts/AuthContext';
import 'bootstrap/scss/bootstrap.scss'
import {Col, Container, Row} from "react-bootstrap";

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
      <AuthProvider>
          <Container fluid>
              <Row>
                  <Col>
                      <App />
                  </Col>
              </Row>
          </Container>
      </AuthProvider>
  </React.StrictMode>,
)

import {Col, Container, Row, Spinner} from "react-bootstrap";

interface LoadingProps {
    message: string;
}

export default function Loading({message}: LoadingProps) {
    return(
        <>
            <Container fluid style={{
                position: 'fixed',
                top: 0,
                left: 0,
                height: '100vh',
                width: '100vw',
                zIndex: 9999,
                backgroundColor: 'rgba(0,0,0,0.2)'
            }}>
                <Row className="h-100">
                    <Col className="d-flex justify-content-center align-items-center text-center">
                        <div>
                            <Spinner animation="border" role="status">
                                <span className="visually-hidden">{message}</span>
                            </Spinner>
                            <p className="mt-2">{message}</p>
                        </div>
                    </Col>
                </Row>
            </Container>
        </>
    );
}